using System;
using Terraria;
using Terraria.ModLoader;

namespace BetterTaxes.Commands
{
    public class SetTaxesCommand : ModCommand
    {
        public override CommandType Type => CommandType.Console;
        public override string Command => "settaxes";
        public override string Usage => "settaxes [player's name] <money in copper coins>";
        public override string Description => "Sets the amount of money in the specified user's tax storage. If no user is given, all players are acted upon.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            int taxAmount = 0;
            if (args.Length > 0)
            {
                try
                {
                    taxAmount = Convert.ToInt32(args[args.Length - 1]);
                }
                catch (OverflowException)
                {
                    caller.Reply("The amount of money specified is too large or too small to be stored.");
                    return;
                }
                catch
                {
                    caller.Reply("Invalid number specified.");
                    return;
                }

                if (taxAmount < 0)
                {
                    caller.Reply("The amount of money the Tax Collector has cannot be negative.");
                    return;
                }

                if (taxAmount > TaxWorld.serverConfig.MoneyCap) taxAmount = TaxWorld.serverConfig.MoneyCap;
            }

            if (args.Length > 1)
            {
                Array.Resize(ref args, args.Length - 1);
                string argsName = string.Join(" ", args).ToLower();

                int playerNum = -1;
                for (int i = 0; i < Main.player.Length; i++)
                {
                    if (Main.player[i].name.ToLower() == argsName)
                    {
                        playerNum = i;
                        break;
                    }
                }

                if (playerNum > -1)
                {
                    ModPacket packet = mod.GetPacket();
                    packet.Write((byte)BetterTaxesMessageType.ForceSetTaxes);
                    packet.Write(playerNum);
                    packet.Write(taxAmount);
                    packet.Send(playerNum);
                }
                else
                {
                    caller.Reply("No player found by the name specified.");
                }
            }
            else
            {
                for (int i = 0; i < Main.player.Length; i++)
                {
                    if (Main.player[i].active)
                    {
                        ModPacket packet = mod.GetPacket();
                        packet.Write((byte)BetterTaxesMessageType.ForceSetTaxes);
                        packet.Write(i);
                        packet.Write(taxAmount);
                        packet.Send(i);
                    }
                }
            }
        }
    }

    /*public class DebugCommand : ModCommand
    {
        public override CommandType Type => CommandType.World | CommandType.Console;
        public override string Command => "debug";
        public override string Usage => "a";
        public override string Description => "b";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Main.dayTime = false;
            Main.time = 16200-(Main.dayRate*60);
            caller.Reply(Main.time.ToString());

            if (Main.netMode == 2)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }
    }

    public class DebugTCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "debug2";
        public override string Usage => "a";
        public override string Description => "b";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            caller.Reply(Main.xMas.ToString());
        }
    }*/
}
