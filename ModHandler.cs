using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace BetterTaxes
{
    public class ModHandler
    {
        internal static Dictionary<string, Dictionary<string, Func<bool>>> delegates = new Dictionary<string, Dictionary<string, Func<bool>>>();

        public static GateParser parser;
        public static Dictionary<string, int> customStatements = new Dictionary<string, int>();

        public static bool NewList(string listName)
        {
            if (delegates.ContainsKey(listName)) delegates.Remove(listName);
            delegates.Add(listName, new Dictionary<string, Func<bool>>());
            return true;
        }

        public static bool NewCondition(string listName, string conditionName, Func<bool> deleg, int recommended)
        {
            if (!delegates.ContainsKey(listName)) NewList(listName);
            if (delegates[listName].ContainsKey(conditionName)) delegates[listName].Remove(conditionName);
            delegates[listName].Add(conditionName, deleg);
            AddRecommended(listName + "." + conditionName, recommended);
            return true;
        }

        public static bool AddRecommended(string statement, int recommended)
        {
            if (customStatements.ContainsKey(statement)) customStatements.Remove(statement);
            customStatements.Add(statement, recommended);
            return true;
        }

        internal static Mod calamityMod;
        public static bool RunConditionByCalamity(string condition)
        {
            if (BetterTaxes.calamityLoaded)
            {
                calamityMod = ModLoader.GetMod("CalamityMod");
                if ((bool)calamityMod.Call("Downed", condition)) return true;
                if ((bool)calamityMod.Call("Difficulty", condition)) return true;

                // backwards compatibility
                switch (condition)
                {
                    case "downedProvidence":
                        return RunConditionByCalamity("providence");
                    case "downedDoG":
                        return RunConditionByCalamity("devourerofgods");
                    case "downedYharon":
                        return RunConditionByCalamity("yharon");
                    case "downedSCal":
                        return RunConditionByCalamity("supremecalamitas");
                    case "revenge":
                        return RunConditionByCalamity("revengeance");
                }
            }
            return false;
        }

        public ModHandler()
        {
            delegates = new Dictionary<string, Dictionary<string, Func<bool>>>();
            parser = new GateParser();
        }
    }
}
