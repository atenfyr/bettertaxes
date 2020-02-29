using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace BetterTaxes
{
    public class GateParser
    {
        public Dictionary<string, bool> invalidMods = new Dictionary<string, bool>();

        public int CalculateRate()
        {
            int taxRate = -1;
            if (TaxWorld.serverConfig.IsFlexible)
            {
                foreach (KeyValuePair<string, int> entry in ModHandler.customStatements)
                {
                    if (entry.Value > taxRate && Interpret(entry.Key)) taxRate = entry.Value;
                }
            }
            foreach (KeyValuePair<string, SpecialInt> entry in TaxWorld.serverConfig.TaxRates)
            {
                if (entry.Value > taxRate && Interpret(entry.Key)) taxRate = entry.Value;
            }
            if (taxRate == -1) throw new InvalidConfigException("No statement evaluated to true. To avoid this error, you should map the statement \"Base.always\" to a value to fall back on");

            if (Main.expertMode && TaxWorld.serverConfig.ExpertModeBoost >= 0) taxRate = (int)(taxRate * TaxWorld.serverConfig.ExpertModeBoost); // Expert mode boost
            if (Main.xMas) taxRate = (int)(taxRate * 1.25); // Christmas boost
            return taxRate;
        }

        public int CalculateNPCCount()
        {
            int npcCount = 0;
            for (int i = 0; i < 200; i++)
            {
                if (Main.npc[i].active && !Main.npc[i].homeless && NPC.TypeToHeadIndex(Main.npc[i].type) > 0) npcCount++;
            }
            return npcCount;
        }

        public static readonly char[] validOpenBrackets = new char[] { '(', '[', '{' };
        public static readonly char[] validCloseBrackets = new char[] { ')', ']', '}' };
        public bool Interpret(string conditions)
        {
            if (!conditions.Contains("(") && !conditions.Contains(")")) return InterpretGates(conditions);

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
                        return NPC.downedGoblins;
                    case "frost":
                    case "frostLegion":
                        return NPC.downedFrost;
                    case "pirates":
                        return NPC.downedPirates;
                    case "martians":
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
                if (chosen_list == "Calamity")
                {
                    return ModHandler.RunConditionByCalamity(chosen_condition);
                }

                // legacy system
                if (!ModHandler.legacyMods.ContainsKey(chosen_list))
                {
                    return false;
                }
                else if (!ModHandler.mods.ContainsKey(chosen_list))
                {
                    ModHandler.mods.Add(chosen_list, ModLoader.GetMod(ModHandler.legacyMods[chosen_list][0]));
                }

                if (ModHandler.legacySynonyms.ContainsKey(chosen_condition)) chosen_condition = ModHandler.legacySynonyms[chosen_condition];
                if (ModHandler.mods.ContainsKey(chosen_list) && ModHandler.mods[chosen_list] != null)
                {
                    ModWorld world = ModHandler.mods[chosen_list].GetModWorld(ModHandler.legacyMods[chosen_list][1]);
                    if (ModHandler.legacyLists[chosen_list].Contains(chosen_condition)) return (bool)world.GetType().GetField(chosen_condition).GetValue(world);
                    throw new InvalidConfigException("Invalid condition \"" + chosen_condition + "\" under list \"" + chosen_list + "\"");
                }
                return false;
            }
            else if (terms.Length == 3) // This probably shouldn't be used, it's much faster and neater for mods to use BetterTaxes's Mod.Call API. I only keep this here for backwards compatibility
            {
                if (invalidMods.ContainsKey(terms[0])) return false;
                Mod customMod = ModLoader.GetMod(terms[0]);
                if (customMod != null)
                {
                    ModWorld customWorld = customMod.GetModWorld(terms[1]);
                    if (customWorld != null)
                    {
                        var thisField = customWorld.GetType().GetField(terms[2]);
                        if (thisField != null) return (bool)thisField.GetValue(customWorld);
                        throw new InvalidConfigException("Could not find field \"" + terms[2] + "\" in mod world \"" + terms[1] + "\" in mod \"" + terms[0] + "\"");
                    }
                    else
                    {
                        throw new InvalidConfigException("Could not find mod world \"" + terms[1] + "\" in mod \"" + terms[0] + "\"");
                    }
                }
                else
                {
                    invalidMods.Add(terms[0], true);
                }
                return false;
            }
            throw new InvalidConfigException("Failed to parse key \"" + condition + "\"");
        }
    }
}
