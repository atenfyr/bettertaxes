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
