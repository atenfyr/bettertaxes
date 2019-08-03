using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BetterTaxes
{
    public static class UsefulThings
    {
        public static string ValueToCoins(int num, string zeroString = "0 copper")
        {
            if (num < 1) return zeroString;
            return Main.ValueToCoins(num);
        }

        public static string ValueToCoinsWithColor(double num, string zeroString = "0 copper")
        {
            if (double.IsPositiveInfinity(num)) return "[c/" + Colors.CoinPlatinum.Hex3() + ":a lot of money]";
            if (double.IsNegativeInfinity(num)) return "[c/" + Colors.CoinCopper.Hex3() + ":not a lot of money]";
            if (double.IsNaN(num)) return "[c/" + Colors.CoinGold.Hex3() + ":something]";
            return ValueToCoinsWithColor((int)num, zeroString);
        }

        public static string ValueToCoinsWithColor(int num, string zeroString = "0 copper")
        {
            string data = ValueToCoins(num, zeroString);
            data = Regex.Replace(data, @"(\d+ platinum)", "[c/" + Colors.CoinPlatinum.Hex3() + ":$1]");
            data = Regex.Replace(data, @"(\d+ gold)", "[c/" + Colors.CoinGold.Hex3() + ":$1]");
            data = Regex.Replace(data, @"(\d+ silver)", "[c/" + Colors.CoinSilver.Hex3() + ":$1]");
            data = Regex.Replace(data, @"(\d+ copper)", "[c/" + Colors.CoinCopper.Hex3() + ":$1]");
            return data;
        }

        public static string SecondsToHMS(int num, string zeroString = "0 seconds")
        {
            if (num < 1) return zeroString;

            string res = "";
            int hours = num / 3600;
            if (hours > 0) res += hours + $" hour{(hours == 1 ? "" : "s")} ";
            num %= 3600;
            int minutes = num / 60;
            if (minutes > 0) res += minutes + $" minute{(minutes == 1 ? "" : "s")} ";
            num %= 60;
            if (num > 0) res += num + $" second{(num == 1 ? "" : "s")} ";

            return res.TrimEnd();
        }

        public static string SecondsToHMSCasual(int num, string zeroString = "a tick")
        {
            if (num < 1) return zeroString;

            string data = SecondsToHMS(num, zeroString);
            if (data == "1 second") return "a second";
            if (data == "1 minute") return "a minute";
            if (data == "1 hour") return "an hour";
            return "every " + data;
        }
    }

    internal enum BetterTaxesMessageType : byte
    {
        ForceSetTaxes
    }

    public class BetterTaxes : Mod
    {
        public static string GithubUserName => "atenfyr";
        public static string GithubProjectName => "bettertaxes";

        internal static BetterTaxes Instance;
        internal Mod herosMod;

        public BetterTaxes()
        {
            Properties = ModProperties.AutoLoadAll;
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            BetterTaxesMessageType msgType = (BetterTaxesMessageType)reader.ReadByte();
            switch (msgType)
            {
                case BetterTaxesMessageType.ForceSetTaxes:
                    int playerNum = reader.ReadInt32();
                    int newTaxValue = reader.ReadInt32();
                    Main.player[playerNum].taxMoney = newTaxValue;
                    if (Main.netMode == 2)
                    {
                        var packet = GetPacket();
                        packet.Write((byte)BetterTaxesMessageType.ForceSetTaxes);
                        packet.Write(playerNum);
                        packet.Write(newTaxValue);
                        packet.Send(playerNum);
                    }
                    break;
                default:
                    Logger.WarnFormat("BetterTaxes.HandlePacket() warning: Unknown message type: {0}", msgType);
                    break;
            }
        }

        public override object Call(params object[] args)
        {
            if (!(args[0] is string)) throw new ModSupportException("First parameter must be a method name");
            string given_method = (string)args[0];
            object[] newArgs = args.Skip(1).ToArray();

            MethodInfo func = typeof(BetterTaxesAPI).GetMethod(given_method, BindingFlags.Public | BindingFlags.Static, null, newArgs.Select(obj => obj.GetType()).ToArray(), null);
            var attr = (ObsoleteAttribute[])func.GetCustomAttributes(typeof(ObsoleteAttribute), false);
            if (attr.Length > 0) throw new ModSupportException(attr[0].Message);
            if (func != null) return func.Invoke(typeof(BetterTaxesAPI), newArgs);
            throw new ModSupportException("Could not find method \"" + given_method + "\" with the arguments specified");
        }

        public override void Load()
        {
            Instance = this;
            new ModHandler();
            herosMod = ModLoader.GetMod("HEROsMod");
        }

        public override void Unload()
        {
            Instance = null;
            TaxWorld.serverConfig = null;
            ModHandler.calamityMod = null;
            ModHandler.calamityDelegate = null;
            ModHandler.parser = null;
            ModHandler.delegates = new Dictionary<string, Dictionary<string, Func<bool>>>();
            ModHandler.mods = new Dictionary<string, Mod>();
            ModHandler.customStatements = new Dictionary<string, int>();
        }

        public override void PostSetupContent()
        {
            try
            {
                if (herosMod != null)
                {
                    HerosIntegration(herosMod);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("BetterTaxes.PostSetupContent() error: " + ex.StackTrace + ex.Message);
            }
        }

        private void HerosIntegration(Mod herosMod)
        {
            herosMod.Call("AddPermission", "ModifyBTConfig", "Modify Better Taxes Config");
        }
    }
}
