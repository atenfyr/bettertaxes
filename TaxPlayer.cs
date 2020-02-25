using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ModLoader;
using static Mono.Cecil.Cil.OpCodes;

namespace BetterTaxes
{
    public class TaxPlayer : ModPlayer
    {
        public override bool Autoload(ref string name)
        {
            IL.Terraria.Player.CollectTaxes += HookAdjustTaxes;
            return base.Autoload(ref name);
        }

        // this is where we adjust the cap and rent values
        private void HookAdjustTaxes(ILContext il)
        {
            var c = new ILCursor(il).Goto(0);
            if (!c.TryGotoNext(i => i.MatchLdsfld(typeof(NPC).GetField(nameof(NPC.taxCollector))))) return;

            // set num1 (rent value) to the result of ModHandler.parser.CalculateRate()
            c.EmitDelegate<Func<int>>(() => ModHandler.parser.CalculateRate());
            c.Emit(Stloc_0);

            // set num2 (cap value) to TaxWorld.serverConfig.MoneyCap
            c.EmitDelegate<Func<int>>(() =>
            {
                if (TaxWorld.serverConfig.MoneyCap < 1) return 2000000000; // for unlimited mode, the absolute cap is 2147483647 but then the math gets a bit weird so we limit it to 2000000000
                return TaxWorld.serverConfig.MoneyCap;
            });
            c.Emit(Stloc_1);
        }

        public override void PostUpdate()
        {
            if (Main.netMode != 2)
            {
                Player.taxRate = (TaxWorld.serverConfig.TimeBetweenPaychecks < 1) ? 1 : (TaxWorld.serverConfig.TimeBetweenPaychecks * 60);

                if (TaxWorld.serverConfig.EnableAutoCollect && !Main.dayTime && Main.time == 16200 && player.taxMoney > 0) // doesn't work with enchanted sundial but that's okay, it'll do it all the next day
                {
                    bool[] bankType = BankHandler.HasBank();

                    bool succeeded = false;
                    if (bankType[0] && !succeeded) succeeded = BankHandler.AddCoins(player.bank, player.taxMoney);
                    if (bankType[1] && !succeeded) succeeded = BankHandler.AddCoins(player.bank2, player.taxMoney);
                    if (bankType[2] && !succeeded) succeeded = BankHandler.AddCoins(player.bank3, player.taxMoney);
                    if (succeeded)
                    {
                        player.taxMoney = 0;
                        BankHandler.LastCheckBank = true;
                    }
                    else
                    {
                        BankHandler.LastCheckBank = false;
                    }
                }
            }
        }
    }
}
