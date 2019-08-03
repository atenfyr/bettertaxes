using System;
using System.Collections.Generic;
using Terraria;
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

        internal static Dictionary<string, Mod> mods = new Dictionary<string, Mod>();
        internal static Dictionary<string, Dictionary<string, Func<bool>>> delegates = new Dictionary<string, Dictionary<string, Func<bool>>>();

        public static GateParser parser;
        public static Dictionary<string, int> customStatements = new Dictionary<string, int>();

        public static bool NewList(string listName)
        {
            if (delegates.ContainsKey(listName)) delegates.Remove(listName);
            delegates.Add(listName, new Dictionary<string, Func<bool>>());
            return true;
        }

        public static bool NewCondition(string listName, string conditionName, Func<bool> deleg)
        {
            if (!delegates.ContainsKey(listName)) NewList(listName);
            if (delegates[listName].ContainsKey(conditionName)) delegates[listName].Remove(conditionName);
            delegates[listName].Add(conditionName, deleg);
            return true;
        }

        public static bool AddStatement(string statement, int rent)
        {
            if (!TaxWorld.serverConfig.IsFlexible) return false;
            if (customStatements.ContainsKey(statement)) customStatements.Remove(statement);
            customStatements.Add(statement, rent);
            return true;
        }

        internal static bool hasCheckedForCalamity = false;
        internal static Mod calamityMod;
        internal static Func<string, bool> calamityDelegate;
        internal static Func<string, bool> calamityDelegate2;
        internal static bool CheckForCalamity(bool setFlag = true)
        {
            if (hasCheckedForCalamity) return calamityDelegate != null;
            hasCheckedForCalamity = setFlag;
            calamityMod = ModLoader.GetMod("CalamityMod");
            if (calamityMod != null)
            {
                object obj = calamityMod.Call("Downed");
                if (obj != null) calamityDelegate = (Func<string, bool>)obj;
                obj = calamityMod.Call("Difficulty");
                if (obj != null) calamityDelegate2 = (Func<string, bool>)obj;
            }
            return calamityDelegate != null;
        }

        public ModHandler()
        {
            delegates = new Dictionary<string, Dictionary<string, Func<bool>>>();
            mods = new Dictionary<string, Mod>();
            parser = new GateParser();

            hasCheckedForCalamity = false;
            hasCheckedForCalamity = CheckForCalamity(false); // if we find it here, we don't need to check again later to see if loading messed up
        }
    }
}
