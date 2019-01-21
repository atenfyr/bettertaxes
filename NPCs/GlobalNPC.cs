using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BetterTaxes.NPCs
{
    public class CustomDialog : GlobalNPC
    {
        public override void GetChat(NPC npc, ref string chat)
        {
            if (npc.type == NPCID.TaxCollector && TaxWorld.addCustomDialog)
            {
                if (Main.rand.Next(5) == 0 && NPC.downedMechBossAny)
                {
                    chat = "Thanks to recent economic prosperity here, I can increase our taxes without as much complaint.";
                }
                if (Main.rand.Next(5) == 0 && !NPC.downedMechBossAny)
                {
                    chat = "I expect that the loot some of those \"powerful monsters\" offer will contribute to the economy enough for me to extort more money from your citizens.";
                }
                if (Main.rand.Next(5) == 0 && TaxWorld.taxCap >= 1000000) // 1 platinum
                {
                    chat = "Since I'm so nice, instead of pocketing everything more than what I can handle, I'm going to toss it on the floor until you come a-knocking.";
                }
                if (Main.rand.Next(5) == 0 && TaxWorld.taxTimer <= 36000) // 10 minutes
                {
                    chat = "How come you expect your money so often?";
                }
            }
        }
    }
}