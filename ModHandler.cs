using System;
using System.Collections.Generic;
using Terraria.ModLoader;

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
}
