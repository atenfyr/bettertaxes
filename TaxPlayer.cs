using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameInput;

namespace BetterTaxes {
    public class TaxPlayer : ModPlayer {
        public int taxRate = Item.buyPrice(copper: 50); // amount given per paycheck
        public int taxTimer = 3600; // time between paychecks, in frames
        public int taxCap = Item.buyPrice(platinum: 10); // max the guy can hold
        public int currentTaxes = 0;
        public int taxWait = 0;
        public bool isJustZero = false;

        public override void PreUpdate() {
            if (Main.netMode != 2) {
                //// increase taxes the further along you are
                if (NPC.downedMoonlord) {
                    this.taxRate = Item.buyPrice(silver: 10);
                } else if (NPC.downedGolemBoss) {
                    this.taxRate = Item.buyPrice(silver: 5);
                } else if (NPC.downedPlantBoss) {
                    this.taxRate = Item.buyPrice(silver: 2);
                } else if (NPC.downedMechBossAny) {
                    this.taxRate = Item.buyPrice(silver: 1);
                } else {
                    this.taxRate = Item.buyPrice(copper: 50);
                }

                //// taxes
                this.taxWait += Main.dayRate;
                if (this.taxWait >= this.taxTimer && NPC.savedTaxCollector) {
                    int npcCount = 0;
                    for (int i = 0; i < 200; i++) {
                        if (Main.npc[i].active && !Main.npc[i].homeless && NPC.TypeToHeadIndex(Main.npc[i].type) > 0) {
                            npcCount++;
                        }
                    }
                    this.taxWait = 0;
                    this.currentTaxes += (this.taxRate*npcCount);
                }

                // enforce cap
                if (this.currentTaxes > this.taxCap) {
                    this.currentTaxes = this.taxCap;
                }

                // when paid, make sure to reset it
                if (Main.player[Main.myPlayer].taxMoney != 0) {
                    this.isJustZero = false;
                }
                if (Main.player[Main.myPlayer].taxMoney == 0 && !isJustZero) {
                    this.currentTaxes = 0;
                    this.isJustZero = true;
                }

                Main.player[Main.myPlayer].taxMoney = this.currentTaxes;
            }
        }

        public override void clientClone(ModPlayer clientClone) {
            TaxPlayer clone = clientClone as TaxPlayer;
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)player.whoAmI);
            packet.Send(toWho, fromWho);
        }

        public override TagCompound Save() {
            return new TagCompound {
                {"taxes", currentTaxes},
                {"internalZero", isJustZero}
            };
        }

        public override void Load(TagCompound tag) {
            currentTaxes = tag.GetInt("taxes");
            isJustZero = tag.GetBool("internalZero");
        }
    }
}
