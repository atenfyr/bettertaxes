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
                int biggest_taxes_stored = 0;
                for (int i = 0; i < 255; i++)
				{
					Player user = Main.player[i];
					if (!user.active)
					{
						continue;
					}
                    if (user.taxMoney > biggest_taxes_stored)
                    {
                        biggest_taxes_stored = user.taxMoney;
                    }
                }

                int npcCount = 0;
                int homelessNpcCount = 0;
                for (int i = 0; i < 200; i++)
                {
                    if (Main.npc[i].active && NPC.TypeToHeadIndex(Main.npc[i].type) > 0)
                    {
                        npcCount++;
                    }
                    if (Main.npc[i].homeless)
                    {
                        homelessNpcCount++;
                    }
                }

                if (Main.rand.Next(7) == 0 && (float)homelessNpcCount/npcCount >= 0.5) // at least half the population is homeless
                {
                    chat = "Do you expect me to charge the homeless rent? Bah!";
                }
                if (Main.rand.Next(7) == 0 && homelessNpcCount == npcCount) // everyone is homeless
                {
                    chat = "Our income right now is zero, all thanks to your mistreatment of your own citizens! Bah!";
                }
                if (Main.rand.Next(7) == 0 && biggest_taxes_stored >= TaxWorld.taxCap/10 && biggest_taxes_stored >= 500000) // more than a tenth of the cap and at least 50 gold
                {
                    chat = "Bah! I've half a mind to keep all this extra coin for myself!";
                }
                if (Main.rand.Next(7) == 0 && NPC.downedMechBossAny) // a mechanical boss has been killed
                {
                    chat = "More money for the both of us, thanks to your mass murder of \"monsters!\"";
                }
                if (Main.rand.Next(7) == 0 && !NPC.downedMechBossAny) // we haven't killed a mechanical boss yet
                {
                    chat = "If you're feeling genocidal, the loot some of those \"powerful monsters\" offer might contribute to the economy enough for me to extort more money from your citizens.";
                }
                if (Main.rand.Next(7) == 0 && TaxWorld.taxTimer <= 36000) // 10 minutes
                {
                    chat = "How come you expect your money so often?";
                }
            }
        }
    }
}