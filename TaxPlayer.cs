using System;
using System.Collections.Generic;
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

        public static readonly Dictionary<string, string[]> legacyMods = new Dictionary<string, string[]> {
            { "Thorium", new string[2] { "ThoriumMod", "ThoriumWorld" } }
        };

        public static Dictionary<string, Mod> mods = new Dictionary<string, Mod>();

        public static Dictionary<string, Dictionary<string, Func<bool>>> delegates = new Dictionary<string, Dictionary<string, Func<bool>>>();
        public static Func<string, bool> calamityDelegate;

        public static GateParser parser;

        public static void NewList(string list_name)
        {
            if (delegates.ContainsKey(list_name)) delegates.Remove(list_name);
            delegates.Add(list_name, new Dictionary<string, Func<bool>>());
        }

        public static void NewCondition(string list_name, string condition, Func<bool> delegatef)
        {
            if (!delegates.ContainsKey(list_name)) NewList(list_name);
            delegates[list_name].Add(condition, delegatef);
        }

        public ModHandler()
        {
            delegates = new Dictionary<string, Dictionary<string, Func<bool>>>();
            mods = new Dictionary<string, Mod>();
            parser = new GateParser();

            Mod calamityMod = ModLoader.GetMod("CalamityMod");
            if (calamityMod != null) calamityDelegate = (Func<string, bool>)calamityMod.Call("Downed");

            foreach (KeyValuePair<string, string[]> entry in legacyMods)
            {
                mods.Add(entry.Key, ModLoader.GetMod(entry.Value[0]));
            }
        }
    }

    public class TaxPlayer : ModPlayer
    {
        public int taxRate = 0;
        public int currentTaxes = 0;
        public int taxWait = 0;

        public override void PreUpdate()
        {
            if (Main.netMode != 2 && NPC.AnyNPCs(NPCID.TaxCollector))
            {
                taxWait += Main.dayRate;
                if (taxWait >= TaxWorld.taxTimer)
                {
                    taxWait = 0;

                    // we don't need to update the tax storage if we've already hit the cap
                    if (currentTaxes < TaxWorld.taxCap)
                    {
                        // determines the number of town NPCs in the world
                        int npcCount = 0;
                        for (int i = 0; i < 200; i++)
                        {
                            if (Main.npc[i].active && !Main.npc[i].homeless && NPC.TypeToHeadIndex(Main.npc[i].type) > 0) npcCount++;
                        }

                        // we have to check the tax rate we should apply every single time an update is due so that the tax rate updates if a boss is killed, but .GetField is super quick after the first time so this shouldn't be a huge problem for custom configs
                        taxRate = -1;
                        foreach (KeyValuePair<string, int> entry in TaxWorld.taxes)
                        {
                            if (entry.Value > taxRate && ModHandler.parser.Interpret(entry.Key)) taxRate = entry.Value;
                        }
                        if (taxRate == -1) throw new InvalidConfigException("No statement evaluated to true. To avoid this error, you should map the statement \"Base.always\" to a value to fall back on");

                        currentTaxes += taxRate * npcCount;
                    }

                    if (currentTaxes > TaxWorld.taxCap)
                    {
                        currentTaxes = TaxWorld.taxCap;
                    }
                }

                player.taxMoney = currentTaxes;
            }
        }

        public override void clientClone(ModPlayer clientClone)
        {
            TaxPlayer clone = clientClone as TaxPlayer;
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)player.whoAmI);
            packet.Send(toWho, fromWho);
        }

        public override TagCompound Save()
        {
            return new TagCompound {
                {"taxes", currentTaxes},
            };
        }

        public override void Load(TagCompound tag)
        {
            currentTaxes = tag.GetInt("taxes");
        }
    }
}
