using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BetterTaxes
{
    public static class UsefulThings
    {
        public static string ValueToCoins(int num, string zeroString = "0 copper")
        {
            if (zeroString == "0 copper") zeroString = "0 " + Language.GetTextValue("LegacyInterface.18");
            if (num < 1) return zeroString;
            return Main.ValueToCoins(num);
        }

        public static string ValueToCoinsWithColor(double num, string zeroString = "0 copper")
        {
            if (zeroString == "0 copper") zeroString = "0 " + Language.GetTextValue("LegacyInterface.18");
            if (double.IsPositiveInfinity(num)) return "[c/" + Colors.CoinPlatinum.Hex3() + ":" + Language.GetTextValue("Mods.BetterTaxes.Status.ALotOfMoney") + "]";
            if (double.IsNegativeInfinity(num)) return "[c/" + Colors.CoinCopper.Hex3() + ":" + Language.GetTextValue("Mods.BetterTaxes.Status.NotALotOfMoney") + "]";
            if (double.IsNaN(num)) return "[c/" + Colors.CoinGold.Hex3() + ":" + Language.GetTextValue("Mods.BetterTaxes.Status.Unknown") + "]";
            return ValueToCoinsWithColor((int)num, zeroString);
        }

        public static string ValueToCoinsWithColor(int num, string zeroString = "0 copper")
        {
            if (zeroString == "0 copper") zeroString = "0 " + Language.GetTextValue("LegacyInterface.18");
            string data = ValueToCoins(num, zeroString);
            data = Regex.Replace(data, @"(\d+ " + Language.GetTextValue("LegacyInterface.15") + ")", "[c/" + Colors.CoinPlatinum.Hex3() + ":$1]");
            data = Regex.Replace(data, @"(\d+ " + Language.GetTextValue("LegacyInterface.16") + ")", "[c/" + Colors.CoinGold.Hex3() + ":$1]");
            data = Regex.Replace(data, @"(\d+ " + Language.GetTextValue("LegacyInterface.17") + ")", "[c/" + Colors.CoinSilver.Hex3() + ":$1]");
            data = Regex.Replace(data, @"(\d+ " + Language.GetTextValue("LegacyInterface.18") + ")", "[c/" + Colors.CoinCopper.Hex3() + ":$1]");
            return data;
        }

        public static string SecondsToHMS(int num, string zeroString = "0 seconds")
        {
            if (zeroString == "0 seconds") zeroString = "0 " + Language.GetTextValue("Mods.BetterTaxes.Config.Seconds");
            if (num < 1) return zeroString;

            string res = "";
            int hours = num / 3600;
            if (hours == 1) res += hours + " " + Language.GetTextValue("Mods.BetterTaxes.Config.Hour") + " ";
            if (hours > 1) res += hours + " " + Language.GetTextValue("Mods.BetterTaxes.Config.Hours") + " ";
            num %= 3600;
            int minutes = num / 60;
            if (minutes == 1) res += minutes + " " + Language.GetTextValue("Mods.BetterTaxes.Config.Minute") + " ";
            if (minutes > 1) res += minutes + " " + Language.GetTextValue("Mods.BetterTaxes.Config.Minutes") + " ";
            num %= 60;
            if (num == 1) res += num + " " + Language.GetTextValue("Mods.BetterTaxes.Config.Second") + " ";
            if (num > 1) res += num + " " + Language.GetTextValue("Mods.BetterTaxes.Config.Seconds") + " ";

            return res.TrimEnd();
        }

        public static string SecondsToHMSCasual(int num, string zeroString = "1 tick")
        {
            if (zeroString == "1 tick") zeroString = "1 " + Language.GetTextValue("Mods.BetterTaxes.Config.Tick");
            if (num < 1) return zeroString;

            return SecondsToHMS(num, zeroString);
        }

        public static int CalculateNPCCount()
        {
            int npcCount = 0;
            for (int i = 0; i < 200; i++)
            {
                if (Main.npc[i].active && !Main.npc[i].homeless && Main.npc[i].townNPC && Main.npc[i].netID != NPCID.OldMan) npcCount++;
            }
            return npcCount;
        }
    }

    public class BetterTaxes : Mod
    {
        public static string GithubUserName => "nolongerlucky";
        public static string GithubProjectName => "bettertaxes";

        internal static BetterTaxes Instance;
        internal Mod herosMod;
        internal static bool calamityLoaded;
        internal static bool herosLoaded;

        public BetterTaxes()
        {
            ContentAutoloadingEnabled = true;
            GoreAutoloadingEnabled = true;
            MusicAutoloadingEnabled = true;
            BackgroundAutoloadingEnabled = true;
        }

        public override object Call(params object[] args)
        {
            if (!(args[0] is string)) throw new ModSupportException("First parameter must be a method name");
            string given_method = (string)args[0];
            object[] newArgs = args.Skip(1).ToArray();

            MethodInfo func = typeof(BetterTaxesAPI).GetMethod(given_method, BindingFlags.Public | BindingFlags.Static, null, newArgs.Select(obj => obj.GetType()).ToArray(), null);
            var attr = (ObsoleteAttribute[])func.GetCustomAttributes(typeof(ObsoleteAttribute), false);
            if (attr.Length > 0) Logger.Warn("Deprecated method warning: " + attr[0].Message);
            if (func != null) return func.Invoke(typeof(BetterTaxesAPI), newArgs);
            throw new ModSupportException("Could not find method \"" + given_method + "\" with the arguments specified");
        }

        public override void Load()
        {
            Instance = this;
            new ModHandler();
            calamityLoaded = ModLoader.TryGetMod("CalamityMod", out Mod CalamityMod);
            herosLoaded = ModLoader.TryGetMod("HEROsMod", out Mod HEROsMod);
        }

        public override void Unload()
        {
            Instance = null;
            TaxWorld.serverConfig = null;
            ModHandler.calamityMod = null;
            herosMod = null;
            ModHandler.parser = null;
            ModHandler.delegates = new Dictionary<string, Dictionary<string, Func<bool>>>();
            ModHandler.customStatements = new Dictionary<string, int>();
        }

        public override void PostSetupContent()
        {
            /*
            // Thorium support
            Mod thoriumMod = ModLoader.GetMod("ThoriumMod");
            if (thoriumMod != null)
            {
                ModSystem thoriumWorld = thoriumMod.GetModWorld("ThoriumWorld");
                Call("AddList", "Thorium");
                Call("AddKey", "Thorium", "primordials", (Func<bool>)delegate () { return (bool)thoriumWorld.GetType().GetField("downedRealityBreaker").GetValue(thoriumWorld); }, -1);
                Call("AddKey", "Thorium", "ragnarok", (Func<bool>)delegate () { return (bool)thoriumWorld.GetType().GetField("downedRealityBreaker").GetValue(thoriumWorld); }, -1);
                Call("AddKey", "Thorium", "patchwerk", (Func<bool>)delegate () { return (bool)thoriumWorld.GetType().GetField("downedPatchwerk").GetValue(thoriumWorld); }, -1);
                Call("AddKey", "Thorium", "bloom", (Func<bool>)delegate () { return (bool)thoriumWorld.GetType().GetField("downedBloom").GetValue(thoriumWorld); }, -1);
                Call("AddKey", "Thorium", "strider", (Func<bool>)delegate () { return (bool)thoriumWorld.GetType().GetField("downedStrider").GetValue(thoriumWorld); }, -1);
                Call("AddKey", "Thorium", "coznix", (Func<bool>)delegate () { return (bool)thoriumWorld.GetType().GetField("downedFallenBeholder").GetValue(thoriumWorld); }, -1);
                Call("AddKey", "Thorium", "lich", (Func<bool>)delegate () { return (bool)thoriumWorld.GetType().GetField("downedLich").GetValue(thoriumWorld); }, -1);
                Call("AddKey", "Thorium", "abyssion", (Func<bool>)delegate () { return (bool)thoriumWorld.GetType().GetField("downedDepthBoss").GetValue(thoriumWorld); }, -1);
            }*/

            // HERO's Mod support
            if (herosLoaded)
            try {
                HerosIntegration(ModLoader.GetMod("HEROsMod"));
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
