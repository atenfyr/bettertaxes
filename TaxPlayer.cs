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

    public class TaxPlayer : ModPlayer
    {
        internal int taxRate = 0;
        internal int taxWait = 0;
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

                        // we have to check the tax rate we should apply every single time an update is due so that the tax rate updates if a boss is killed, but .GetField is super quick after the first time so this shouldn't be a huge problem for custom configs
                        taxRate = -1;
                        foreach (KeyValuePair<string, CoinValue> entry in TaxWorld.serverConfig.TaxRates)
                        {
                            if (entry.Value > taxRate && ModHandler.parser.Interpret(entry.Key)) taxRate = entry.Value;
                        }
                        if (taxRate == -1) throw new InvalidConfigException("No statement evaluated to true. To avoid this error, you should map the statement \"Base.always\" to a value to fall back on");

                        //if (Main.expertMode && TaxWorld.serverConfig.IsFlexible) taxRate = (int)(taxRate * 1.5);
                        currentTaxes += taxRate * npcCount;
                    }

                    if (TaxWorld.serverConfig.MoneyCap > 0 && currentTaxes > TaxWorld.serverConfig.MoneyCap)
                    {
                        currentTaxes = TaxWorld.serverConfig.MoneyCap;
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
