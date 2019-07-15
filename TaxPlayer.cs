using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BetterTaxes
{
    public class ModHandler
    {
        public static readonly Dictionary<string, string[]> legacyLists = new Dictionary<string, string[]> {
            { "Thorium", new string[7] { "downedRealityBreaker", "downedPatchwerk", "downedBloom", "downedStrider", "downedFallenBeholder", "downedLich", "downedDepthBoss" } }
        };

        public static readonly Dictionary<string, string> legacySynonyms = new Dictionary<string, string> {
            { "ragnarok", "downedRealityBreaker" },
            { "patchwerk", "downedPatchwerk" },
            { "bloom", "downedBloom" },
            { "strider", "downedStrider" },
            { "coznix", "downedFallenBeholder" },
            { "lich", "downedLich" },
            { "abyssion", "downedDepthBoss" }
        };

        public static readonly Dictionary<string, string[]> legacyMods = new Dictionary<string, string[]> {
            { "Thorium", new string[2] { "ThoriumMod", "ThoriumWorld" } }
        };

        public static Dictionary<string, Mod> mods = new Dictionary<string, Mod>();

        public static Dictionary<string, Dictionary<string, Func<bool>>> delegates = new Dictionary<string, Dictionary<string, Func<bool>>>();
        public static Mod calamityMod;
        public static Func<string, bool> calamityDelegate;

        public static Dictionary<string, int> customStatements = new Dictionary<string, int>();
        public static GateParser parser;

        public static bool NewList(string list_name)
        {
            if (delegates.ContainsKey(list_name)) delegates.Remove(list_name);
            delegates.Add(list_name, new Dictionary<string, Func<bool>>());
            return true;
        }

        public static bool NewCondition(string list_name, string condition, Func<bool> delegatef)
        {
            if (!delegates.ContainsKey(list_name)) NewList(list_name);
            if (delegates[list_name].ContainsKey(condition)) delegates[list_name].Remove(condition);
            delegates[list_name].Add(condition, delegatef);
            return true;
        }

        public static bool AddStatement(string statement, int value)
        {
            if (!TaxWorld.serverConfig.IsFlexible) return false;
            if (customStatements.ContainsKey(statement)) customStatements.Remove(statement);
            customStatements.Add(statement, value);
            return true;
        }   

        public ModHandler()
        {
            delegates = new Dictionary<string, Dictionary<string, Func<bool>>>();
            mods = new Dictionary<string, Mod>();
            parser = new GateParser();

            calamityMod = ModLoader.GetMod("CalamityMod");
            if (calamityMod != null) calamityDelegate = (Func<string, bool>)calamityMod.Call("Downed");

            foreach (KeyValuePair<string, string[]> entry in legacyMods)
            {
                mods.Add(entry.Key, ModLoader.GetMod(entry.Value[0]));
            }
        }
    }

    public static class BankHandler
    {
        public static bool LastCheckBank = false;
        public static ushort[] SafeTypes = new ushort[] { TileID.PiggyBank, TileID.Safes, TileID.DefendersForge };
        public static int HasBank(int x, int y)
        {
            if (!WorldGen.StartRoomCheck(x, y)) return -1;
            for (int k = WorldGen.roomY1; k <= WorldGen.roomY2; k++)
            {
                for (int j = WorldGen.roomX1; j <= WorldGen.roomX2; j++)
                {
                    if (Main.tile[j, k] != null && Main.tile[j, k].active())
                    {
                        ushort type = Main.tile[j, k].type;
                        if (SafeTypes.Contains(type)) return type;
                    }
                }
            }

            return -1;
        }

        public static bool CheckIfFull(Chest bank, int type, int amount)
        {
            Item data = new Item();
            data.SetDefaults(type);
            int maxStack = data.maxStack;
            data = null;

            foreach (Item item in bank.item)
            {
                if (item.type == 0 || (item.type == type && item.stack < maxStack)) return false;
            }
            return true;
        }

        public static bool AddItem(Chest bank, int slot, int type, int amount)
        {
            if (amount < 1) return false;
            if (slot < 0) slot += bank.item.Length;
            if (slot > (bank.item.Length - 1)) slot -= bank.item.Length;
            if (CheckIfFull(bank, type, amount)) return false;
            if (bank.item[slot].type == 0)
            {
                bank.item[slot].SetDefaults(type);
                bank.item[slot].stack = amount;
                return true;
            }
            else if (bank.item[slot].type == type)
            {
                if (bank.item[slot].stack + amount >= bank.item[slot].maxStack)
                {
                    int remainder = (bank.item[slot].stack + amount) % 100;

                    int newType = type;
                    int newAmount = bank.item[slot].stack + amount - remainder;
                    if (type > 70 && type < 74)
                    {
                        newType++;
                        newAmount /= 100;
                    }

                    bank.item[slot].stack = remainder;
                    return AddItem(bank, slot + 1, newType, newAmount);
                }
                else
                {
                    bank.item[slot].stack += amount;
                }
            }
            else
            {
                return AddItem(bank, slot + 1, type, amount);
            }
            return true;
        }

        public static int AddCoins(Chest bank, int amount)
        {
            int allCoins = amount;
            int[] coinsArr = Utils.CoinsSplit(amount);
            int type = 71;
            int factor = 1;
            for (int i = 0; i < coinsArr.Length; i++)
            {
                int amn = coinsArr[i];

                if (amn > 0)
                {
                    bool hasSolved = false;
                    for (int j = 0; j < bank.item.Length; j++)
                    {
                        if (bank.item[j].type == type)
                        {
                            if (AddItem(bank, j, type, amn)) allCoins -= amn * factor;
                            hasSolved = true;
                            break;
                        }
                    }

                    if (!hasSolved)
                    {
                        for (int j = 0; j < bank.item.Length; j++)
                        {
                            if (bank.item[j].type == 0)
                            {
                                if (AddItem(bank, j, type, amn)) allCoins -= amn * factor;
                                break;
                            }
                        }
                    }
                }

                type++;
                factor *= 100;
            }
            return allCoins;
        }
    }

    public class TaxPlayer : ModPlayer
    {
        internal int taxWait = 0;
        internal int autoCollectWait = 0;
        internal bool hasAlreadyDone = false;
        public int currentTaxes = 0;

        public override void PreUpdate()
        {
            if (Main.netMode != 2 && NPC.AnyNPCs(NPCID.TaxCollector))
            {
                taxWait += Main.dayRate;
                if (taxWait >= TaxWorld.serverConfig.TimeBetweenPaychecks * 60)
                {
                    taxWait -= TaxWorld.serverConfig.TimeBetweenPaychecks * 60;

                    // we don't need to update the tax storage if we've already hit the cap
                    if (TaxWorld.serverConfig.MoneyCap < 1 || currentTaxes < TaxWorld.serverConfig.MoneyCap)
                    {
                        // determines the number of town NPCs in the world
                        int npcCount = 0;
                        for (int i = 0; i < 200; i++)
                        {
                            if (Main.npc[i].active && !Main.npc[i].homeless && NPC.TypeToHeadIndex(Main.npc[i].type) > 0) npcCount++;
                        }

                        int taxRate = ModHandler.parser.CalculateRate();
                        currentTaxes += taxRate * npcCount;
                    }

                    if (TaxWorld.serverConfig.MoneyCap > 0 && currentTaxes > TaxWorld.serverConfig.MoneyCap)
                    {
                        currentTaxes = TaxWorld.serverConfig.MoneyCap;
                    }

                    // if the user has set the cap to unlimited we need to check for integer overflow
                    if (TaxWorld.serverConfig.MoneyCap < 1)
                    {
                        if ((long)currentTaxes > int.MaxValue) currentTaxes = int.MaxValue;
                    }
                }

                if (hasAlreadyDone && Main.dayTime) hasAlreadyDone = false;
                if (TaxWorld.serverConfig.EnableAutoCollect && !Main.dayTime && Main.time >= 16200 && currentTaxes > 0 && !hasAlreadyDone)
                { 
                    int collector = NPC.FindFirstNPC(NPCID.TaxCollector);
                    int bankType = BankHandler.HasBank(Main.npc[collector].homeTileX, Main.npc[collector].homeTileY - 1);
                    if (bankType >= 0)
                    {
                        switch (bankType)
                        {
                            case TileID.PiggyBank:
                                currentTaxes = BankHandler.AddCoins(player.bank, currentTaxes);
                                break;
                            case TileID.Safes:
                                currentTaxes = BankHandler.AddCoins(player.bank2, currentTaxes);
                                break;
                            case TileID.DefendersForge:
                                currentTaxes = BankHandler.AddCoins(player.bank3, currentTaxes);
                                break;
                        }
                        BankHandler.LastCheckBank = true;
                        hasAlreadyDone = true;
                    }
                    else
                    {
                        BankHandler.LastCheckBank = false;
                    }
                }

                player.taxMoney = currentTaxes;
            }
        }

        public override TagCompound Save()
        {
            return new TagCompound {
                { "taxes", currentTaxes}
            };
        }

        public override void Load(TagCompound tag)
        {
            currentTaxes = tag.GetInt("taxes");
        }
    }
}
