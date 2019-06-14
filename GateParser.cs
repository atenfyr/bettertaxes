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

        public bool Interpret(string conditions)
        {
            // go through parentheses
            int firstBracket = conditions.IndexOf("(");
            int secondBracket = conditions.LastIndexOf(")");
            if (firstBracket < 0 && secondBracket > -1) throw new InvalidConfigException("Unable to find matching start parenthesis in statement \"" + conditions + "\"");
            if (firstBracket > -1)
            {
                if (secondBracket < 0) throw new InvalidConfigException("Unable to find matching end parenthesis in statement \"" + conditions + "\"");
                string textToReplace = conditions.Substring(firstBracket+1, secondBracket-firstBracket-1);
                conditions = conditions.Substring(0, firstBracket) + (Interpret(textToReplace) ? "true" : "false") + conditions.Substring(secondBracket+1);
            }

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
                    }
                }
            }

            return InterpretCondition(string.Join(" ", terms.ToArray()));
        }

        private bool InterpretCondition(string condition)
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
                    case "expertMode":
                        return Main.expertMode;
                    case "crimson":
                        return WorldGen.crimson;
                    case "corruption": // equivalent to "not Base.crimson"
                        return !WorldGen.crimson;
                }
                throw new InvalidConfigException("Invalid condition \"" + terms[1] + "\" under list \"Base\"");
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
                    if (ModHandler.calamityDelegate != null)
                    {
                        if (ModHandler.calamityDelegate(chosen_condition)) return true;
                        switch (chosen_condition) // backwards compatibility
                        {
                            case "providence":
                            case "downedProvidence":
                                return ModHandler.calamityDelegate("providence");
                            case "dog":
                            case "downedDoG":
                                return ModHandler.calamityDelegate("devourerofgods");
                            case "yharon":
                            case "downedYharon":
                                return ModHandler.calamityDelegate("yharon");
                            case "scal":
                            case "downedSCal":
                                return ModHandler.calamityDelegate("supremecalamitas");
                        }
                        return false;
                    }
                    return false;
                }

                // legacy system
                if (!ModHandler.legacyMods.ContainsKey(chosen_list)) return false;
                if (ModHandler.legacySynonyms.ContainsKey(chosen_condition)) chosen_condition = ModHandler.legacySynonyms[chosen_condition];
                if (ModHandler.mods.ContainsKey(chosen_list) && ModHandler.mods[chosen_list] != null)
                {
                    ModWorld world = ModHandler.mods[chosen_list].GetModWorld(ModHandler.legacyMods[chosen_list][1]);
                    foreach (string boss in ModHandler.legacyLists[chosen_list])
                    {
                        if (boss == chosen_condition) return (bool)world.GetType().GetField(boss).GetValue(world);
                    }
                    throw new InvalidConfigException("Invalid condition \"" + chosen_condition + "\" under list \"" + chosen_list + "\"");
                }
                return false;
            }
            else if (terms.Length == 3) // note that this will probably add some lag to world start times
            {
                if (invalidMods.ContainsKey(terms[0])) return false;
                Mod customMod = ModLoader.GetMod(terms[0]);
                if (customMod != null)
                {
                    ModWorld customWorld = customMod.GetModWorld(terms[1]);
                    if (customWorld != null)
                    {
                        var thisField = customWorld.GetType().GetField(terms[2]);
                        if (thisField != null)
                        {
                            return (bool)thisField.GetValue(customWorld);
                        }
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
