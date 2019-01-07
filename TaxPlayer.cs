using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BetterTaxes
{
    public class TaxPlayer : ModPlayer
    {
        public int taxRate = 0; // amount given per paycheck
        public int currentTaxes = 0;
        public int taxWait = 0;
        public bool isJustZero = false;

        public bool interpretGates(string conditions)
        {
            List<string> terms = conditions.Split(' ').ToList();
            
            for (int i = 0; i < terms.Count; i++)
            {
                switch(terms[i])
                {
                    case "not":
                        terms[i] = (interpretCondition(terms[i])) ? "Base.never" : "Base.always";
                        terms.RemoveAt(i+1);
                        break;
                }
            }

            bool hasChanged = true;
            while (hasChanged)
            {
                hasChanged = false;
                for (int i = 1; i < terms.Count-1; i++) // first and last terms can't be a gate
                {
                    switch(terms[i])
                    {
                        case "and":
                            terms[i-1] = (interpretCondition(terms[i-1]) && interpretCondition(terms[i+1])) ? "Base.always" : "Base.never";
                            terms.RemoveAt(i+1);
                            terms.RemoveAt(i);
                            hasChanged = true;
                            break;
                        case "or":
                            terms[i-1] = (interpretCondition(terms[i-1]) || interpretCondition(terms[i+1])) ? "Base.always" : "Base.never";
                            terms.RemoveAt(i+1);
                            terms.RemoveAt(i);
                            hasChanged = true;
                            break;
                    }
                }
            }

            return interpretCondition(string.Join(" ", terms.ToArray()));
        }

        public bool interpretCondition(string condition)
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
            }
            else if (terms.Length == 3) // note that this will probably add some lag to world start times
            {
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
            }
            return false;
        }

        public override void PreUpdate()
        {
            if (Main.netMode != 2)
            {
                //// increase taxes the further along you are. we have to check every single time so that it updates if a boss is killed and the mod is not reloaded, but .GetField is super quick after first time so this shouldn't be a problem for custom configs
                taxRate = 0;

                foreach (KeyValuePair<string, int> entry in TaxWorld.taxes)
                {
                    if (entry.Value > taxRate && entry.Key.Contains(".")) // custom entries in config
                    {
                        if (interpretGates(entry.Key))
                        {
                            taxRate = entry.Value;
                        }
                    }
                }

                //// taxes
                taxWait += Main.dayRate;
                if (taxWait >= TaxWorld.taxTimer && NPC.savedTaxCollector)
                {
                    // this is the exact same code that Terraria uses to determine the # of town NPCs in the world
                    int npcCount = 0;
                    for (int i = 0; i < 200; i++)
                    {
                        if (Main.npc[i].active && !Main.npc[i].homeless && NPC.TypeToHeadIndex(Main.npc[i].type) > 0)
                        {
                            npcCount++;
                        }
                    }
                    taxWait = 0;
                    currentTaxes += (taxRate * npcCount);
                }

                // enforce cap
                if (currentTaxes > TaxWorld.taxCap)
                {
                    currentTaxes = TaxWorld.taxCap;
                }

                // when paid, make sure to reset it
                if (Main.player[Main.myPlayer].taxMoney != 0)
                {
                    isJustZero = false;
                }
                if (Main.player[Main.myPlayer].taxMoney == 0 && !isJustZero)
                {
                    currentTaxes = 0;
                    isJustZero = true;
                }

                // taxMoney is the amount of money the tax collector has stored for this player. the display dialog actually does support platinum despite that never happening in vanilla, so we can just override the stored value every single frame so that the old system does nothing
                Main.player[Main.myPlayer].taxMoney = currentTaxes;
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
