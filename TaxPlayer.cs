using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BetterTaxes
{
    public static class TaxConstants
    {
        public static readonly Dictionary<string, string[]> validLists = new Dictionary<string, string[]> {
            { "Calamity", new string[] { "downedProvidence", "downedDoG", "downedYharon", "downedSCal", "downedBumble", "downedCryogen", "downedLeviathan", "downedAstrageldon", "downedStarGod", "downedPlaguebringer", "downedScavenger" } },
            { "Thorium", new string[] { "downedRealityBreaker", "downedPatchwerk", "downedBloom", "downedStrider", "downedFallenBeholder", "downedLich", "downedDepthBoss" } }
        };

        public static readonly Dictionary<string, string[]> validMods = new Dictionary<string, string[]> {
            { "Calamity", new string[2] { "CalamityMod", "CalamityWorld" } },
            { "Thorium", new string[2] { "ThoriumMod", "ThoriumWorld" } }
        };

        public static Dictionary<string, Mod> mods = new Dictionary<string, Mod>();

        static TaxConstants()
        {
            foreach (KeyValuePair<string, string[]> entry in validMods)
            {
                mods.Add(entry.Key, ModLoader.GetMod(entry.Value[0]));
            }
        }
    }

    public static class GateParser
    {
        private static Dictionary<string, bool> invalidMods = new Dictionary<string, bool>();

        public static bool Interpret(string conditions)
        {
            List<string> terms = conditions.Split(' ').ToList();

            for (int i = 0; i < terms.Count; i++)
            {
                switch (terms[i])
                {
                    case "not":
                        terms[i] = (InterpretCondition(terms[i])) ? "Base.never" : "Base.always";
                        terms.RemoveAt(i + 1);
                        break;
                }
            }

            bool hasChanged = true;
            while (hasChanged)
            {
                hasChanged = false;
                for (int i = 1; i < terms.Count - 1; i++) // first and last terms can't be a gate
                {
                    switch (terms[i])
                    {
                        case "and":
                            terms[i - 1] = (InterpretCondition(terms[i - 1]) && InterpretCondition(terms[i + 1])) ? "Base.always" : "Base.never";
                            terms.RemoveAt(i + 1);
                            terms.RemoveAt(i);
                            hasChanged = true;
                            break;
                        case "or":
                            terms[i - 1] = (InterpretCondition(terms[i - 1]) || InterpretCondition(terms[i + 1])) ? "Base.always" : "Base.never";
                            terms.RemoveAt(i + 1);
                            terms.RemoveAt(i);
                            hasChanged = true;
                            break;
                    }
                }
            }

            return InterpretCondition(string.Join(" ", terms.ToArray()));
        }

        public static bool InterpretCondition(string condition)
        {
            string[] terms = condition.Split('.');

            if (terms.Length == 2 && terms[0] == "Base") // example: Base.downedMoonlord
            {
                switch (terms[1])
                {
                    case "always":
                        return true;
                    case "never":
                        return false;
                    case "downedMoonlord":
                        return NPC.downedMoonlord;
                    case "downedGolemBoss":
                        return NPC.downedGolemBoss;
                    case "downedPlantBoss":
                        return NPC.downedPlantBoss;
                    case "downedMechBossAny":
                        return NPC.downedMechBossAny;
                    case "downedMechBossAll":
                        return NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
                    case "downedAncientCultist":
                        return NPC.downedAncientCultist;
                    case "expertMode":
                        return Main.expertMode;
                    case "crimson":
                        return WorldGen.crimson;
                }
                throw new InvalidConfigException("Invalid condition \"" + terms[1] + "\" under list \"Base\"");
            }
            else if (terms.Length == 2)
            {
                string chosen_list = terms[0];
                string chosen_condition = terms[1];
                if (!TaxConstants.validMods.ContainsKey(chosen_list)) throw new InvalidConfigException("Invalid list \"" + chosen_list + "\"");
                if (TaxConstants.mods.ContainsKey(chosen_list) && TaxConstants.mods[chosen_list] != null)
                {
                    ModWorld world = TaxConstants.mods[chosen_list].GetModWorld(TaxConstants.validMods[chosen_list][1]);
                    foreach (string boss in TaxConstants.validLists[chosen_list])
                    {
                        if (boss == chosen_condition)
                        {
                            return (bool)world.GetType().GetField(boss).GetValue(world);
                        }
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
            throw new InvalidConfigException("Failed to parse flag \"" + condition + "\"");
        }
    }
    public class TaxPlayer : ModPlayer
    {
        public int taxRate = 0;
        public int currentTaxes = 0;
        public int taxWait = 0;

        public override void PreUpdate()
        {
            if (Main.netMode != 2 && NPC.savedTaxCollector)
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
                        taxRate = 0;
                        foreach (KeyValuePair<string, int> entry in TaxWorld.taxes)
                        {
                            if (entry.Value > taxRate && GateParser.Interpret(entry.Key)) taxRate = entry.Value;
                        }

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
