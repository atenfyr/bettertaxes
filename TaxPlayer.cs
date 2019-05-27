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
        public static Mod calamityMod;
        public static ModWorld calamityWorld;
        public static Mod thoriumMod;
        public static ModWorld thoriumWorld;

        static TaxConstants()
        {
            calamityMod = ModLoader.GetMod("CalamityMod");
            if (calamityMod != null)
            {
                calamityWorld = calamityMod.GetModWorld("CalamityWorld");
            }
            thoriumMod = ModLoader.GetMod("ThoriumMod");
            if (thoriumMod != null)
            {
                thoriumWorld = thoriumMod.GetModWorld("ThoriumWorld");
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
                throw new InvalidConfigException("Invalid condition \"" + terms[1] + "\" under list \"Base\". See https://github.com/atenfyr/bettertaxes/blob/master/CONFIG.md.");
            }
            else if (terms.Length == 2 && terms[0] == "Calamity") // example: Calamity.downedProvidence
            {
                if (TaxConstants.calamityWorld != null)
                {
                    switch (terms[1])
                    {
                        case "downedProvidence":
                            return (bool)TaxConstants.calamityWorld.GetType().GetField("downedProvidence").GetValue(TaxConstants.calamityWorld);
                        case "downedDoG":
                            return (bool)TaxConstants.calamityWorld.GetType().GetField("downedDoG").GetValue(TaxConstants.calamityWorld);
                        case "downedYharon":
                            return (bool)TaxConstants.calamityWorld.GetType().GetField("downedYharon").GetValue(TaxConstants.calamityWorld);
                        case "downedSCal":
                            return (bool)TaxConstants.calamityWorld.GetType().GetField("downedSCal").GetValue(TaxConstants.calamityWorld);
                    }
                    throw new InvalidConfigException("Invalid condition \"" + terms[1] + "\" under list \"Calamity\". See https://github.com/atenfyr/bettertaxes/blob/master/CONFIG.md.");
                }
                return false;
            }
            else if (terms.Length == 2 && terms[0] == "Thorium") // example: Thorium.downedRealityBreaker
            {
                if (TaxConstants.thoriumWorld != null)
                {
                    switch (terms[1])
                    {
                        case "downedRealityBreaker":
                            return (bool)TaxConstants.thoriumWorld.GetType().GetField("downedRealityBreaker").GetValue(TaxConstants.thoriumWorld);
                    }
                    throw new InvalidConfigException("Invalid condition \"" + terms[1] + "\" under list \"Thorium\". See https://github.com/atenfyr/bettertaxes/blob/master/CONFIG.md.");
                }
                return false;
            }
            else if (terms.Length == 2)
            {
                throw new InvalidConfigException("Invalid list \"" + terms[0] + "\".");
            }
            else if (terms.Length == 3) // note that this will probably add some lag to world start times
            {
                if (invalidMods.ContainsKey(terms[0])) return false;
                Mod customMod = ModLoader.GetMod(terms[0]);
                if (customMod != null)
                {
                    ModWorld customWorld = customMod.GetModWorld(terms[1]);
                    if (customWorld == null)
                    {
                        throw new Exception("Could not find mod world \"" + terms[1] + "\" in mod \"" + terms[0] + "\"");
                    }
                    var thisField = customWorld.GetType().GetField(terms[2]);
                    if (thisField == null)
                    {
                        throw new Exception("Could not find field \"" + terms[2] + "\" in mod world \"" + terms[1] + "\" in mod \"" + terms[0] + "\"");
                    }
                    return (bool)thisField.GetValue(customWorld);
                }
                else
                {
                    invalidMods.Add(terms[0], true);
                }
                return false;
            }
            throw new InvalidConfigException("Failed to parse flag \"" + condition + "\". See https://github.com/atenfyr/bettertaxes/blob/master/CONFIG.md.");
        }
    }
    public class TaxPlayer : ModPlayer
    {
        public int taxRate = 0;
        public int currentTaxes = 0;
        public int taxWait = 0;
        private bool isJustZero = false;

        public override void PreUpdate()
        {
            if (Main.netMode != 2)
            {
                taxWait += Main.dayRate;
                if (taxWait >= TaxWorld.taxTimer && NPC.savedTaxCollector)
                {
                    taxWait = 0;

                    // we don't need to update the tax storage if we've already hit the cap
                    if (currentTaxes < TaxWorld.taxCap)
                    {
                        // determines the number of town NPCs in the world
                        int npcCount = 0;
                        for (int i = 0; i < 200; i++)
                        {
                            if (Main.npc[i].active && !Main.npc[i].homeless && NPC.TypeToHeadIndex(Main.npc[i].type) > 0)
                            {
                                npcCount++;
                            }
                        }

                        // we have to check the tax rate we should apply every single time an update is due so that the tax rate updates if a boss is killed, but .GetField is super quick after the first time so this shouldn't be a huge problem for custom configs
                        taxRate = 0;
                        foreach (KeyValuePair<string, int> entry in TaxWorld.taxes)
                        {
                            if (entry.Value > taxRate) // custom entries in config
                            {
                                if (GateParser.Interpret(entry.Key))
                                {
                                    taxRate = entry.Value;
                                }
                            }
                        }

                        currentTaxes += taxRate * npcCount;
                    }
                    else
                    {
                        currentTaxes = TaxWorld.taxCap;
                    }
                }

                // when paid, make sure to reset it
                if (player.taxMoney != 0)
                {
                    isJustZero = false;
                }
                if (player.taxMoney == 0 && !isJustZero)
                {
                    currentTaxes = 0;
                    isJustZero = true;
                }

                // taxMoney is the amount of money the tax collector has stored for this player. the display dialog actually does support platinum despite that never happening in vanilla, so we can just override the stored value every single frame so that the old system does nothing
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
                {"internalZero", isJustZero}
            };
        }

        public override void Load(TagCompound tag)
        {
            currentTaxes = tag.GetInt("taxes");
            isJustZero = tag.GetBool("internalZero");
        }
    }
}
