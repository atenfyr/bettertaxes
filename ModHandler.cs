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
        public static bool RunConditionByCalamity(string condition, bool isSecondRun = false)
        {
            if (BetterTaxes.calamityLoaded)
            {
                calamityMod = ModLoader.GetMod("CalamityMod");
                if ((bool)calamityMod.Call("GetBossDowned", condition.ToLower())) return true;
                if ((bool)calamityMod.Call("DifficultyActive", condition.ToLower())) return true;
                if (!isSecondRun)
                {
                    switch(condition)
                    {
                        case "leviathan":
                            return RunConditionByCalamity("anahitaleviathan", true);
                        case "plaguebringer":
                            return RunConditionByCalamity("plaguebringergoliath", true);
                        case "bumblebirb":
                            return RunConditionByCalamity("dragonfolly", true);
                        case "dog":
                            return RunConditionByCalamity("devourerofgods", true);
                        case "scal":
                            return RunConditionByCalamity("supremecalamitas", true);
                        case "revenge":
                            return RunConditionByCalamity("revengeance", true);

                    }
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
