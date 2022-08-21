using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
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
        public override bool IsLoadingEnabled(Mod mod) => false;

        private void HookAdjustButton(ILContext il)
        {
            var c = new ILCursor(il).Goto(0);
            if (!c.TryGotoNext(i => i.MatchLdcI4(NPCID.TaxCollector))) return;
            if (!c.TryGotoNext(i => i.Match(Bne_Un))) return;

            c.Index++;
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
                SoundEngine.PlaySound(SoundID.MenuTick, new Vector2(-1, -1));

                int rawTax = ModHandler.parser.CalculateRate();
                int adjustedTax = rawTax * UsefulThings.CalculateNPCCount();
                double rate = TaxWorld.serverConfig.TimeBetweenPaychecks / Main.dayRate;
                Main.npcChatText = Language.GetTextValue("Mods.BetterTaxes.Status.StatusMessage").Replace(@"%1", UsefulThings.ValueToCoinsWithColor(rawTax)).Replace(@"%2", UsefulThings.SecondsToHMSCasual((int)rate)).Replace(@"%3", UsefulThings.ValueToCoinsWithColor(adjustedTax * (3600 / rate)));
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

                if (Main.rand.Next(2) == 0)
                {
                    bool hasChosenDialog = false;
                    while (!hasChosenDialog)
                    {
                        int chosenDialog = Main.rand.Next(9); // 0 - 8
                        switch(chosenDialog)
                        {
                            case 0:
                                if ((float)homelessNpcCount / npcCount >= 0.5) // at least half the population is homeless
                                {
                                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.0"); hasChosenDialog = true;
                                }
                                break;
                            case 1:
                                if (homelessNpcCount == npcCount) // everyone is homeless
                                {
                                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.1"); hasChosenDialog = true;
                                }
                                break;
                            case 2:
                                if (taxAmount >= TaxWorld.serverConfig.MoneyCap / 10 && taxAmount >= 500000) // more than a tenth of the cap and at least 50 gold
                                {
                                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.2"); hasChosenDialog = true;
                                }
                                break;
                            case 3:
                                if (TaxWorld.serverConfig.TaxRates.ContainsKey("Base.mechAny") && NPC.downedMechBossAny) // a mechanical boss has been killed
                                {
                                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.3"); hasChosenDialog = true;
                                }
                                break;
                            case 4:
                                if (TaxWorld.serverConfig.TaxRates.ContainsKey("Base.mechAny") && !NPC.downedMechBossAny) // we haven't killed a mechanical boss yet
                                {
                                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.4"); hasChosenDialog = true;
                                }
                                break;
                            case 5:
                                if (TaxWorld.serverConfig.TimeBetweenPaychecks <= 1440) // 24 *minutes* (or one in-game day)
                                {
                                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.5"); hasChosenDialog = true;
                                }
                                break;
                            case 6:
                                if (TaxWorld.serverConfig.EnableAutoCollect && !BankHandler.LastCheckBank) // player doesn't have autocollection set up
                                {
                                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.6"); hasChosenDialog = true;
                                }
                                break;
                            case 7:
                                if (DialogUtils.CheckIfModExists("VendingMachines")) // has vending machines mod
                                {
                                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.7"); hasChosenDialog = true;
                                }
                                break;
                            case 8:
                                if (Main.xMas) // currently christmas season!
                                {
                                    chat = Language.GetTextValue("Mods.BetterTaxes.Dialog.8"); hasChosenDialog = true;
                                }
                                break;
                            default:
                                chat = Language.GetTextValue("tModLoader.DefaultTownNPCChat"); hasChosenDialog = true;
                                break;
                        }

                    }
                }

            }
        }
    }
}