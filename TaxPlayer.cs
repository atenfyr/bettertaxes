using MonoMod.Cil;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Mono.Cecil.Cil.OpCodes;

namespace BetterTaxes
{
    public class TaxPlayer : ModPlayer
    {
        internal bool hasAlreadyDone = false;

        public override bool Autoload(ref string name)
        {
            IL.Terraria.Player.CollectTaxes += HookAdjustTaxes;
            return base.Autoload(ref name);
        }

        // this is where we adjust the cap and rent values
        public void HookAdjustTaxes(ILContext il)
        {
            var c = new ILCursor(il).Goto(0);
            if (!c.TryGotoNext(i => i.MatchLdsfld(typeof(NPC).GetField(nameof(NPC.taxCollector))))) return;

            var label = il.DefineLabel();

            // set num1 (rent value) to the result of ModHandler.parser.CalculateRate()
            c.Emit(Ldsfld, typeof(ModHandler).GetField(nameof(ModHandler.parser)));
            c.Emit(Callvirt, typeof(GateParser).GetMethod(nameof(GateParser.CalculateRate)));
            c.Emit(Stloc_0);

            // set num2 (cap value) to TaxWorld.serverConfig.MoneyCap
            c.Emit(Ldsfld, typeof(TaxWorld).GetField(nameof(TaxWorld.serverConfig)));
            c.Emit(Ldfld, typeof(Config).GetField(nameof(Config.MoneyCap)));
            MethodInfo implicitCastMethod = typeof(SpecialInt)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == "op_Implicit")
                .Where(m => m.ReturnType == typeof(int))
                .Where(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(SpecialInt))
                .FirstOrDefault();
            c.Emit(Call, implicitCastMethod); // cast TaxWorld.serverConfig.MoneyCap from SpecialInt to int
            c.Emit(Stloc_1);

            // if the cap is 0 or negative (aka no cap) set it to the maximum value we can possibly store
            c.Emit(Ldloc_1);
            c.Emit(Ldc_I4, 1);
            c.Emit(Bge_S, label);
            c.Emit(Ldc_I4, int.MaxValue);
            c.Emit(Stloc_1);

            c.MarkLabel(label);
        }

        public override void PreUpdate()
        {
            if (Main.netMode != 2)
            {
                Player.taxRate = TaxWorld.serverConfig.TimeBetweenPaychecks * 60;

                if (hasAlreadyDone && Main.dayTime) hasAlreadyDone = false;
                if (TaxWorld.serverConfig.EnableAutoCollect && !Main.dayTime && Main.time >= 16200 && player.taxMoney > 0 && !hasAlreadyDone)
                { 
                    int bankType = BankHandler.HasBank();
                    if (bankType >= 0)
                    {
                        switch (bankType)
                        {
                            case TileID.PiggyBank:
                                player.taxMoney = BankHandler.AddCoins(player.bank, player.taxMoney);
                                break;
                            case TileID.Safes:
                                player.taxMoney = BankHandler.AddCoins(player.bank2, player.taxMoney);
                                break;
                            case TileID.DefendersForge:
                                player.taxMoney = BankHandler.AddCoins(player.bank3, player.taxMoney);
                                break;
                        }
                        BankHandler.LastCheckBank = true;
                        hasAlreadyDone = true;
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
