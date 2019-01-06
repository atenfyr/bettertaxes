using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BetterTaxes {
    public class TaxPlayer : ModPlayer {
        public int taxRate = TaxWorld.taxes["PostWall"]; // amount given per paycheck
        public int currentTaxes = 0;
        public int taxWait = 0;
        public bool isJustZero = false;

        public override void PreUpdate() {
            if (Main.netMode != 2) {
                //// increase taxes the further along you are
                if (NPC.downedMoonlord) {
                    taxRate = TaxWorld.taxes["PostMoonLord"];
                } else if (NPC.downedGolemBoss) {
                    taxRate = TaxWorld.taxes["PostGolem"];
                } else if (NPC.downedPlantBoss) {
                    taxRate = TaxWorld.taxes["PostPlantera"];
                } else if (NPC.downedMechBossAny) {
                    taxRate = TaxWorld.taxes["PostAnyMechBoss"];
                } else {
                    taxRate = TaxWorld.taxes["PostWall"];
                }

                //// taxes
                this.taxWait += Main.dayRate;
                if (this.taxWait >= TaxWorld.taxTimer && NPC.savedTaxCollector) {
                    // this is the exact same code that Terraria uses to determine the # of town NPCs alive
                    int npcCount = 0;
                    for (int i = 0; i < 200; i++) {
                        if (Main.npc[i].active && !Main.npc[i].homeless && NPC.TypeToHeadIndex(Main.npc[i].type) > 0) {
                            npcCount++;
                        }
                    }
                    taxWait = 0;
                    currentTaxes += (this.taxRate*npcCount);
                }

                // enforce cap
                if (currentTaxes > TaxWorld.taxCap) {
                    currentTaxes = TaxWorld.taxCap;
                }

                // when paid, make sure to reset it
                if (Main.player[Main.myPlayer].taxMoney != 0) {
                    isJustZero = false;
                }
                if (Main.player[Main.myPlayer].taxMoney == 0 && !isJustZero) {
                    currentTaxes = 0;
                    isJustZero = true;
                }

                // taxMoney is the amount of money the tax collector has stored for this player. the display dialog actually does support platinum despite that never happening in vanilla, so we can just override the stored value every single frame so that the old system does nothing
                Main.player[Main.myPlayer].taxMoney = currentTaxes;
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
