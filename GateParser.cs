using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace BetterTaxes
{
    public class GateParser
    {
        public int CalculateRate()
        {
            int taxRate = -1;
            foreach (KeyValuePair<string, SpecialInt> entry in TaxWorld.serverConfig.TaxRates)
            {
                if (entry.Value > taxRate && Interpret(entry.Key)) taxRate = entry.Value;
            }
            if (taxRate == -1) throw new InvalidConfigException("No statement evaluated to true. To avoid this error, you should map the statement \"Base.always\" to a value to fall back on");

            if (Main.expertMode && TaxWorld.serverConfig.ExpertModeBoost >= 0) taxRate = (int)(taxRate * TaxWorld.serverConfig.ExpertModeBoost); // Expert mode boost
            if (Main.xMas) taxRate = (int)(taxRate * 1.1); // Christmas boost
            return taxRate;
        }

        public static readonly char[] validOpenBrackets = new char[] { '(', '[', '{' };
        public static readonly char[] validCloseBrackets = new char[] { ')', ']', '}' };
        public bool Interpret(string conditions)
        {
            // 1st pass: make sure everything is valid
            Stack<int> bracketStack = new Stack<int>();
            for (int i = 0; i < conditions.Length; i++)
            {
                if (validOpenBrackets.Contains(conditions[i])) bracketStack.Push(i);
                if (validCloseBrackets.Contains(conditions[i])) bracketStack.Pop();
            }
            if (bracketStack.Count > 0) throw new InvalidConfigException("Failed to parse parentheses in statement \"" + conditions + "\"");

            // 2nd pass: break it down bit by bit until we have an answer
            bool hasChanged = true;
            while (hasChanged)
            {
                hasChanged = false;
                bracketStack = new Stack<int>();
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (validOpenBrackets.Contains(conditions[i]))
                    {
                        hasChanged = true;
                        bracketStack.Push(i);
                    }
                    if (validCloseBrackets.Contains(conditions[i]))
                    {
                        hasChanged = true;
                        int pos = bracketStack.Pop();
                        string textToReplace = conditions.Substring(pos + 1, i - pos - 1);
                        conditions = conditions.Substring(0, pos) + (Interpret(textToReplace) ? "true" : "false") + conditions.Substring(i + 1);
                        break;
                    }
                }
            }

            return InterpretGates(conditions);
        }

        public bool InterpretGates(string conditions)
        {
            List<string> terms = conditions.Split(' ').ToList();

            // gates that take 1 input
            bool hasChanged = true;
            while (hasChanged)
            {
                hasChanged = false;
                for (int i = 0; i < terms.Count; i++)
                {
                    if (terms[i] == "not")
                    {
                        terms[i] = InterpretCondition(terms[i + 1]) ? "false" : "true";
                        terms.RemoveAt(i + 1);
                        hasChanged = true;
                        break;
                    }
                }
            }

            // gates that take 2 inputs
            hasChanged = true;
            while (hasChanged)
            {
                hasChanged = false;
                for (int i = 1; i < terms.Count - 1; i++) // first and last terms can't be a gate
                {
                    switch (terms[i])
                    {
                        case "and":
                            terms[i - 1] = (InterpretCondition(terms[i - 1]) && InterpretCondition(terms[i + 1])) ? "true" : "false";
                            terms.RemoveAt(i + 1);
                            terms.RemoveAt(i);
                            hasChanged = true;
                            break;
                        case "or":
                            terms[i - 1] = (InterpretCondition(terms[i - 1]) || InterpretCondition(terms[i + 1])) ? "true" : "false";
                            terms.RemoveAt(i + 1);
                            terms.RemoveAt(i);
                            hasChanged = true;
                            break;
                        case "xor":
                            terms[i - 1] = (InterpretCondition(terms[i - 1]) ^ InterpretCondition(terms[i + 1])) ? "true" : "false";
                            terms.RemoveAt(i + 1);
                            terms.RemoveAt(i);
                            hasChanged = true;
                            break;
                    }
                }
            }

            return InterpretCondition(string.Join(" ", terms.ToArray()));
        }

        public bool InterpretCondition(string condition)
        {
            if (condition == "true") return true;
            if (condition == "false") return false;

            string[] terms = condition.Split('.');

            if (terms.Length == 2 && terms[0] == "Base") // example: Base.downedMoonlord
            {
                switch (terms[1])
                {
                    case "always":
                        return true;
                    case "never":
                        return false;
                    case "moonlord":
                    case "downedMoonlord":
                        return NPC.downedMoonlord;
                    case "golem":
                    case "downedGolemBoss":
                        return NPC.downedGolemBoss;
                    case "plantera":
                    case "downedPlantBoss":
                        return NPC.downedPlantBoss;
                    case "mechAny":
                    case "downedMechBossAny":
                        return NPC.downedMechBossAny;
                    case "mechAll":
                    case "downedMechBossAll":
                        return NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
                    case "cultist":
                    case "downedAncientCultist":
                        return NPC.downedAncientCultist;

                    // miscellaneous
                    case "expert":
                    case "expertMode":
                        return Main.expertMode;
                    case "crimson":
                        return WorldGen.crimson;
                    case "corruption": // equivalent to "not Base.crimson"
                        return !WorldGen.crimson;
                }
                throw new InvalidConfigException("Invalid condition \"" + terms[1] + "\" under list \"Base\"");
            }
            else if (terms.Length == 2 && terms[0] == "Invasion")
            {
                switch (terms[1])
                {
                    case "goblins":
                    case "downedGoblins":
                        return NPC.downedGoblins;
                    case "frost":
                    case "frostLegion":
                    case "downedFrost":
                        return NPC.downedFrost;
                    case "pirates":
                    case "downedPirates":
                        return NPC.downedPirates;
                    case "martians":
                    case "downedMartians":
                        return NPC.downedMartians;
                }
                throw new InvalidConfigException("Invalid condition \"" + terms[1] + "\" under list \"Invasion\"");
            }
            else if (terms.Length == 2)
            {
                string chosen_list = terms[0];
                string chosen_condition = terms[1];

                // delegate system
                if (ModHandler.delegates.ContainsKey(chosen_list))
                {
                    Dictionary<string, Func<bool>> checkers = ModHandler.delegates[chosen_list];
                    if (checkers != null)
                    {
                        if (!checkers.ContainsKey(chosen_condition)) throw new InvalidConfigException("Invalid condition \"" + chosen_condition + "\" under list \"" + chosen_list + "\"");
                        return checkers[chosen_condition]();
                    }
                    return false;
                }

                // special case for calamity
                if (chosen_list == "Calamity") return ModHandler.RunConditionByCalamity(chosen_condition);
                return false;
            }
            throw new InvalidConfigException("Failed to parse key \"" + condition + "\"");
        }
    }
}
