using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BetterTaxes.NPCs {
	public class GlobalNPCs : GlobalNPC {
		public override void GetChat(NPC npc, ref string chat) {
			if (npc.type == NPCID.TaxCollector) {
                if (Main.rand.Next(3) == 0) {
                    int dialog = Main.rand.Next(3);
                    switch (dialog) {
                        case 0:
                            if (NPC.downedMechBossAny) {
                                chat = "Thanks to recent economic prosperity here, I can increase our taxes without as much complaint.";
                            } else {
                                chat = "I expect that the loot some of those \"powerful monsters\" offer will contribute to the economy enough for me to extort more money from your citizens.";
                            }
                            break;
                        case 1:
                            chat = "Since I'm so nice, instead of pocketing everything more than what I can handle, I'm going to toss it on the floor until you come a-knocking.";
                            break;
                        case 2:
                            chat = "The fact that you expect your money more than once a day is depressing, but I suppose I've got to comply.";
                            break;
                    }
                }
			}
        }
    }
}