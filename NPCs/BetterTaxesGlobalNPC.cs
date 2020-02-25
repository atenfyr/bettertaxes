using MonoMod.Cil;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
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
            c.EmitDelegate<Func<string>>(() => Language.GetTextValue("Mods.BetterTaxes.Status.Status"));
            c.Emit(Stloc_S, (byte)10);
            c.MarkLabel(label);
        }

        public override void OnChatButtonClicked(NPC npc, bool firstButton)
        {
            if (npc.type == NPCID.TaxCollector && !firstButton && TaxWorld.serverConfig.AddCustomDialog)
            {
                Main.PlaySound(12, -1, -1, 1, 1f, 0f);

                int rawTax = ModHandler.parser.CalculateRate();
                int adjustedTax = rawTax * ModHandler.parser.CalculateNPCCount();
                double rate = TaxWorld.serverConfig.TimeBetweenPaychecks / Main.dayRate;
                Main.npcChatText = Language.GetTextValue("Mods.BetterTaxes.Status.StatusMessage").Replace(@"%1", UsefulThings.ValueToCoinsWithColor(rawTax)).Replace(@"%2", UsefulThings.SecondsToHMSCasual((int)rate)).Replace(@"%3", UsefulThings.ValueToCoinsWithColor(adjustedTax * (3600 / rate)));
                //Main.npcChatText = $"Well, rent's getting charged at {UsefulThings.ValueToCoinsWithColor(rawTax)} every {UsefulThings.SecondsToHMSCasual((int)rate)} per citizen, which is netting you {UsefulThings.ValueToCoinsWithColor(adjustedTax * (3600 / rate))} an hour. Does that answer your question?";
            }
        }

        public override void GetChat(NPC npc, ref string chat)
        {
            if (npc.type == NPCID.TaxCollector && TaxWorld.serverConfig.AddCustomDialog)
            {
                int taxAmount = Main.LocalPlayer.taxMoney;

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
                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.0");
                }
                if (Main.rand.Next(7) == 0 && homelessNpcCount == npcCount) // everyone is homeless
                {
                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.1");
                }
                if (Main.rand.Next(7) == 0 && taxAmount >= TaxWorld.serverConfig.MoneyCap / 10 && taxAmount >= 500000) // more than a tenth of the cap and at least 50 gold
                {
                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.2");
                }
                if (Main.rand.Next(7) == 0 && TaxWorld.serverConfig.TaxRates.ContainsKey("Base.mechAny") && NPC.downedMechBossAny) // a mechanical boss has been killed
                {
                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.3");
                }
                if (Main.rand.Next(7) == 0 && TaxWorld.serverConfig.TaxRates.ContainsKey("Base.mechAny") && !NPC.downedMechBossAny) // we haven't killed a mechanical boss yet
                {
                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.4");
                }
                if (Main.rand.Next(7) == 0 && TaxWorld.serverConfig.TimeBetweenPaychecks <= 1440) // 24 *minutes* (or one in-game day)
                {
                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.5");
                }
                if (Main.rand.Next(7) == 0 && TaxWorld.serverConfig.EnableAutoCollect && !BankHandler.LastCheckBank)
                {
                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.6");
                }
                if (Main.rand.Next(7) == 0 && DialogUtils.CheckIfModExists("VendingMachines"))
                {
                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.7");
                }
            }
        }
    }
}