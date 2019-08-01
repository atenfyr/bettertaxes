using MonoMod.Cil;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Mono.Cecil.Cil.OpCodes;

namespace BetterTaxes.NPCs
{
    internal static class DialogUtils
    {
        internal static int[] permanentlyHomelessNPCs = new int[] { NPCID.OldMan, NPCID.TravellingMerchant, NPCID.SkeletonMerchant };

        internal static bool CheckIfModExists(string name)
        {
            return ModLoader.GetMod(name) != null;
        }
    }

    public class BetterTaxesGlobalNPC : GlobalNPC
    {
        public override bool Autoload(ref string name)
        {
            IL.Terraria.Main.GUIChatDrawInner += HookAdjustButton;
            return base.Autoload(ref name);
        }

        private void HookAdjustButton(ILContext il)
        {
            var c = new ILCursor(il).Goto(0);
            if (!c.TryGotoNext(i => i.MatchLdcI4(NPCID.TaxCollector))) return;

            c.Index += 2;
            ILLabel label = il.DefineLabel();
            c.EmitDelegate<Func<bool>>(() => TaxWorld.serverConfig.AddCustomDialog);
            c.Emit(Brfalse_S, label);
            c.Emit(Ldstr, "Status");
            c.Emit(Stloc_S, (byte)10);
            c.MarkLabel(label);
        }

        public override void OnChatButtonClicked(NPC npc, bool firstButton)
        {
            if (npc.type == NPCID.TaxCollector && !firstButton)
            {
                Main.PlaySound(12, -1, -1, 1, 1f, 0f);

                int rawTax = ModHandler.parser.CalculateRate();
                int adjustedTax = rawTax * ModHandler.parser.CalculateNPCCount();
                double rate = TaxWorld.serverConfig.TimeBetweenPaychecks / Main.dayRate;
                Main.npcChatText = $"Well, rent's getting charged at {UsefulThings.ValueToCoinsWithColor(rawTax)} {UsefulThings.SecondsToHMSCasual((int)rate)} per citizen, which is netting you {UsefulThings.ValueToCoinsWithColor(adjustedTax * (3600 / rate))} an hour. Does that answer your question?";
            }
        }

        public override void GetChat(NPC npc, ref string chat)
        {
            if (npc.type == NPCID.TaxCollector && TaxWorld.serverConfig.AddCustomDialog)
            {
                int taxAverage = 0;
                int playerCount = 0;
                for (int i = 0; i < 255; i++)
                {
                    Player user = Main.player[i];
                    if (!user.active) continue;
                    playerCount++;
                    taxAverage += user.taxMoney;
                }
                taxAverage /= playerCount;

                int npcCount = 0;
                int homelessNpcCount = 0;
                for (int i = 0; i < 200; i++)
                {
                    if (DialogUtils.permanentlyHomelessNPCs.Contains(Main.npc[i].type)) continue;
                    if (Main.npc[i].active && NPC.TypeToHeadIndex(Main.npc[i].type) > 0) npcCount++;
                    if (Main.npc[i].homeless) homelessNpcCount++;
                }

                if (Main.rand.Next(7) == 0 && (float)homelessNpcCount/npcCount >= 0.5) // at least half the population is homeless
                {
                    chat = "Do you expect me to charge the homeless rent? Bah!";
                }
                if (Main.rand.Next(7) == 0 && homelessNpcCount == npcCount) // everyone is homeless
                {
                    chat = "Our income right now is zero, all thanks to your mistreatment of your own citizens! Bah!";
                }
                if (Main.rand.Next(7) == 0 && taxAverage >= TaxWorld.serverConfig.MoneyCap / 10 && taxAverage >= 500000) // more than a tenth of the cap and at least 50 gold
                {
                    chat = "Bah! I've half a mind to keep all this extra coin for myself!";
                }
                if (Main.rand.Next(7) == 0 && TaxWorld.serverConfig.TaxRates.ContainsKey("Base.mechAny") && NPC.downedMechBossAny) // a mechanical boss has been killed
                {
                    chat = "More money for the both of us, thanks to your mass murder of \"monsters!\"";
                }
                if (Main.rand.Next(7) == 0 && TaxWorld.serverConfig.TaxRates.ContainsKey("Base.mechAny") && !NPC.downedMechBossAny) // we haven't killed a mechanical boss yet
                {
                    chat = "If you're feeling genocidal, the loot some of those \"powerful monsters\" offer might contribute to the economy enough for me to extort more money from your citizens.";
                }
                if (Main.rand.Next(7) == 0 && TaxWorld.serverConfig.TimeBetweenPaychecks <= 600) // 10 minutes
                {
                    chat = "How come you expect your money so often?";
                }
                if (Main.rand.Next(7) == 0 && TaxWorld.serverConfig.EnableAutoCollect && !BankHandler.LastCheckBank)
                {
                    chat = "If you were to give me something to put your coin into, like a piggy bank, you wouldn't have to talk to me anymore!";
                }
                if (Main.rand.Next(7) == 0 && DialogUtils.CheckIfModExists("VendingMachines"))
                {
                    chat = "You would never harvest my soul, would you?";
                }
            }
        }
    }
}