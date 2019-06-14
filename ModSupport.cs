using log4net;
using System;
using Terraria;

namespace BetterTaxes
{
    public class ModSupport
    {
        public class ModSupportException : Exception
        {
            public static readonly string messageFormat = "BetterTaxes.Call() error: {0}.";
            public ModSupportException()
            {
            }

            public ModSupportException(string message) : base(string.Format(messageFormat, message))
            {
            }

            public ModSupportException(string message, Exception inner) : base(string.Format(messageFormat, message), inner)
            {
            }
        }

        public static object Call(params object[] args)
        {
            if (args.Length <= 0 || !(args[0] is string)) throw new ModSupportException("The first parameter must be a method name");
            string given_method = (string)args[0];
            switch (given_method)
            {
                case "AddList":
                    if (args.Length < 3 || !(args[1] is string)) throw new ModSupportException("Usage: AddList <list name>");
                    ModHandler.NewList((string)args[1]);
                    return true;
                case "AddKey":
                    if (args.Length < 3 || !(args[1] is string) || !(args[2] is string) || !(args[3] is Func<bool>)) throw new ModSupportException("Usage: AddKey <list name> <condition name> <function returning a boolean>");
                    ModHandler.NewCondition((string)args[1], (string)args[2], (Func<bool>)args[3]);
                    return true;
                case "AddStatement":
                    if (args.Length < 3 || !(args[1] is string) || !(args[2] is int)) throw new ModSupportException("Usage: AddStatement <statement> <rent in copper coins>");
                    return TaxWorld.serverConfig.AddStatement((string)args[1], (int)args[2]);
                case "Save":
                    LogManager.GetLogger("BetterTaxes").Warn("BetterTaxes.Call() warning: The method \"Save\" was removed in BetterTaxes 2.0.0 due to the limitations of tModLoader v0.11's ModConfig class.");
                    return false;
                case "GetConfig":
                    if (args.Length < 2) throw new ModSupportException("Usage: GetConfig <config field>");
                    string field = (string)args[1];
                    switch(field)
                    {
                        case "TaxRates":
                            return TaxWorld.serverConfig.TaxRates;
                        case "TimeBetweenPaychecks":
                            return TaxWorld.serverConfig.TimeBetweenPaychecks;
                        case "MoneyCap":
                            return TaxWorld.serverConfig.MoneyCap;
                        case "AddCustomDialog":
                            return TaxWorld.serverConfig.AddCustomDialog;
                        case "IsFlexible":
                            return TaxWorld.serverConfig.IsFlexible;
                    }
                    throw new ModSupportException("No config field found by the name of \"" + field + "\"");
                case "SetTaxes":
                case "ResetPlayerModData": // this alternative way of calling "SetTaxes" is implemented only to follow the guidelines specified by hamstar's Mod Helpers
                    if (args.Length < 2) throw new ModSupportException("Usage: " + given_method + " <player> [tax amount]");
                    Player player = (Player)args[1];
                    TaxPlayer moddedPlayer = player.GetModPlayer<TaxPlayer>();
                    int newAmount = 0;
                    if (args.Length > 2) newAmount = (int)args[2];
                    if (moddedPlayer != null)
                    {
                        moddedPlayer.currentTaxes = newAmount;
                        player.taxMoney = newAmount;
                        return true;
                    }
                    throw new ModSupportException("Invalid player specified");
            }
            throw new ModSupportException("No method found by the name of \"" + given_method + "\"");
        }
    }
}