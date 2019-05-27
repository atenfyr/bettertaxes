using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BetterTaxes.Commands
{
    public class InfoCommand : ModCommand
    {
        public static string ValueToCoins(int num)
        {
            if (num == 0)
            {
                return "0 copper";
            }
            return Main.ValueToCoins(num);
        }

        public override CommandType Type
        {
            get { return CommandType.Chat; }
        }

        public override string Command
        {
            get { return "taxinfo"; }
        }

        public override string Usage
        {
            get { return "/taxinfo"; }
        }

        public override string Description
        {
            get { return "Provides information about tax rates currently in place"; }
        }

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            int npcCount = 0;
            for (int i = 0; i < 200; i++)
            {
                if (Main.npc[i].active && !Main.npc[i].homeless && NPC.TypeToHeadIndex(Main.npc[i].type) > 0)
                {
                    npcCount++;
                }
            }

            int taxRate = 0;
            foreach (KeyValuePair<string, int> entry in TaxWorld.taxes)
            {
                if (entry.Value > taxRate) // custom entries in config
                {
                    if (GateParser.Interpret(entry.Key))
                    {
                        taxRate = entry.Value;
                    }
                }
            }

            int rate = TaxWorld.taxTimer / 60;
            caller.Reply("Tax rate: " + ValueToCoins(taxRate * npcCount) + " per " + TimeSpan.FromSeconds(rate / Main.dayRate).ToString(@"mm\:ss") + "\nUnadjusted tax rate: " + ValueToCoins(taxRate) + " per " + TimeSpan.FromSeconds(rate).ToString(@"mm\:ss") + " per NPC\nHoused NPC Count: " + npcCount, Color.Yellow);
        }
    }
}
