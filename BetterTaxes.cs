using Terraria;
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
    }

    class BetterTaxes : Mod
    {
        public static string GithubUserName => "atenfyr";
        public static string GithubProjectName => "bettertaxes";

        public BetterTaxes()
        {
            Properties = ModProperties.AutoLoadAll;
        }

        public override object Call(params object[] args)
        {
            return ModSupport.Call(args);
        }

        public override void Load()
        {
            new ModHandler(); // resets all the delegates etc
        }

        public override void Unload()
        {
            TaxWorld.serverConfig = null;
        }
    }
}