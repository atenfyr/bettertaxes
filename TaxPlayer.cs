using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BetterTaxes
{
    public class TaxPlayer : ModPlayer
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            On.Terraria.Player.CollectTaxes += HookAdjustTaxes;
            return base.IsLoadingEnabled(mod);
        }
        private void HookAdjustTaxes(On.Terraria.Player.orig_CollectTaxes orig, Player self)
        {
            int cap = TaxWorld.serverConfig.MoneyCap;
            if (cap < 1) cap = 2000000000;
            if (!NPC.taxCollector || self.taxMoney >= cap) return;

            self.taxMoney += ModHandler.parser.CalculateRate() * UsefulThings.CalculateNPCCount();
            if (self.taxMoney > cap) self.taxMoney = cap;
        }

        private bool hasCollected = false;
        public override void PostUpdate()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Player.taxRate = (TaxWorld.serverConfig.TimeBetweenPaychecks < 1) ? 1 : (TaxWorld.serverConfig.TimeBetweenPaychecks * 60);

                if (Main.dayTime && hasCollected) hasCollected = false;
                if (TaxWorld.serverConfig.EnableAutoCollect && !Main.dayTime && !hasCollected && Main.time >= 16200 && Player.taxMoney > 0)
                {
                    hasCollected = true;
                    bool succeeded = false;
                    if (TaxWorld.ClientBanksList[0] && !succeeded) succeeded = BankHandler.AddCoins(Player.bank, Player.taxMoney);
                    if (TaxWorld.ClientBanksList[1] && !succeeded) succeeded = BankHandler.AddCoins(Player.bank2, Player.taxMoney);
                    if (TaxWorld.ClientBanksList[2] && !succeeded) succeeded = BankHandler.AddCoins(Player.bank3, Player.taxMoney);
                    if (succeeded)
                    {
                        Player.taxMoney = 0;
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
