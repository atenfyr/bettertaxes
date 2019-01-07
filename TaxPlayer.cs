using System;
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
                        string[] terms = entry.Key.Split('.');
                        if (terms.Length == 2 && terms[0] == "Base") // example: Base.downedMoonlord
                        {
                            switch (terms[1])
                            {
                                case "always":
                                    taxRate = entry.Value;
                                    break;
                                case "downedMoonlord":
                                    if (NPC.downedMoonlord)
                                    {
                                        taxRate = entry.Value;
                                    }
                                    break;
                                case "downedGolemBoss":
                                    if (NPC.downedGolemBoss)
                                    {
                                        taxRate = entry.Value;
                                    }
                                    break;
                                case "downedPlantBoss":
                                    if (NPC.downedPlantBoss)
                                    {
                                        taxRate = entry.Value;
                                    }
                                    break;
                                case "downedMechBossAny":
                                    if (NPC.downedMechBossAny)
                                    {
                                        taxRate = entry.Value;
                                    }
                                    break;
                                case "downedMechBossAll":
                                    if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                                    {
                                        taxRate = entry.Value;
                                    }
                                    break;
                                case "downedAncientCultist":
                                    if (NPC.downedAncientCultist)
                                    {
                                        taxRate = entry.Value;
                                    }
                                    break;
                                case "expertMode":
                                    if (Main.expertMode)
                                    {
                                        taxRate = entry.Value;
                                    }
                                    break;
                                case "crimson":
                                    if (WorldGen.crimson)
                                    {
                                        taxRate = entry.Value;
                                    }
                                    break;
                                case "corruption":
                                    if (!WorldGen.crimson)
                                    {
                                        taxRate = entry.Value;
                                    }
                                    break;
                            }
                        }
                        else if (terms.Length == 3)
                        {
                            // example: CalamityMod.CalamityWorld.downedDoG
                            // note that this will probably add some lag to world start times
                            Mod customMod = ModLoader.GetMod(terms[0]);
                            if (customMod != null)
                            {
                                ModWorld customWorld = customMod.GetModWorld(terms[1]);
                                if ((bool)customWorld.GetType().GetField(terms[2]).GetValue(customWorld))
                                {
                                    taxRate = entry.Value;
                                }
                            }
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
