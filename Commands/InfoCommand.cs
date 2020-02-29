using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace BetterTaxes.Commands
{
    public class InfoCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat | CommandType.Console;
        public override string Command => "taxinfo";
        public override string Usage => "/taxinfo";
        public override string Description => "Provides information about tax rates currently in place.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            int npcCount = ModHandler.parser.CalculateNPCCount();
            int taxRate = ModHandler.parser.CalculateRate();
            long rate = TaxWorld.serverConfig.TimeBetweenPaychecks;
            caller.Reply("Tax rate: " + UsefulThings.ValueToCoins(taxRate * npcCount) + " per " + TimeSpan.FromSeconds(rate / Main.dayRate).ToString(@"mm\:ss") + "\nUnadjusted tax rate: " + UsefulThings.ValueToCoins(taxRate) + " per " + TimeSpan.FromSeconds(rate).ToString(@"mm\:ss") + " per NPC\nHoused NPC Count: " + npcCount, Color.Yellow);
        }
    }
}
