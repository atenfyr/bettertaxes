using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
            if (NPC.savedTaxCollector)
            {
                int npcCount = 0;
                for (int i = 0; i < 200; i++)
                {
                    if (Main.npc[i].active && !Main.npc[i].homeless && NPC.TypeToHeadIndex(Main.npc[i].type) > 0) npcCount++;
                }

                int taxRate = -1;
                foreach (KeyValuePair<string, CoinValue> entry in TaxWorld.serverConfig.TaxRates)
                {
                    if (entry.Value > taxRate && ModHandler.parser.Interpret(entry.Key)) taxRate = entry.Value;
                }
                if (taxRate == -1) throw new InvalidConfigException("No statement evaluated to true. To avoid this error, you should map the statement \"Base.always\" to a value to fall back on");

                long rate = TaxWorld.serverConfig.TimeBetweenPaychecks;
                caller.Reply("Tax rate: " + UsefulThings.ValueToCoins(taxRate * npcCount) + " per " + TimeSpan.FromSeconds(rate / Main.dayRate).ToString(@"mm\:ss") + "\nUnadjusted tax rate: " + UsefulThings.ValueToCoins(taxRate) + " per " + TimeSpan.FromSeconds(rate).ToString(@"mm\:ss") + " per NPC\nHoused NPC Count: " + npcCount, Color.Yellow);
            }
            else
            {
                caller.Reply("The Tax Collector has not yet been saved in this world!", Color.OrangeRed);
            }
        }
    }
}
