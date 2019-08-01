using log4net;
using System;
using System.Reflection;
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

        public class InvalidSyntaxException : Exception
        {
            public static readonly string messageFormat = "Usage: {0}";
            public InvalidSyntaxException()
            {
            }

            public InvalidSyntaxException(string message) : base(string.Format(messageFormat, message))
            {
            }

            public InvalidSyntaxException(string message, Exception inner) : base(string.Format(messageFormat, message), inner)
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
                    if (args.Length < 2 || !(args[1] is string)) throw new InvalidSyntaxException("AddList <list name>");
                    return ModHandler.NewList((string)args[1]);
                case "AddKey":
                    if (args.Length < 4 || !(args[1] is string) || !(args[2] is string) || !(args[3] is Func<bool>)) throw new InvalidSyntaxException("AddKey <list name> <condition name> <function returning a boolean>");
                    return ModHandler.NewCondition((string)args[1], (string)args[2], (Func<bool>)args[3]);
                case "AddStatement":
                    if (args.Length < 3 || !(args[1] is string) || !(args[2] is int)) throw new InvalidSyntaxException("AddStatement <statement> <rent in copper coins>");
                    return ModHandler.AddStatement((string)args[1], (int)args[2]);
                case "Save":
                    LogManager.GetLogger("BetterTaxes").Warn("BetterTaxes.Call() warning: The method \"Save\" was removed in BetterTaxes 2.0.0 due to the limitations of tModLoader v0.11's ModConfig class.");
                    return false;
                case "GetConfig":
                    if (args.Length < 2) throw new InvalidSyntaxException("GetConfig <config field>");
                    string field = (string)args[1];
                    FieldInfo data = TaxWorld.serverConfig.GetType().GetField(field);
                    if (data != null) return data.GetValue(null);
                    /*switch(field)
                    {
                        case "TaxRates":
                            return TaxWorld.serverConfig.TaxRates;
                        case "TimeBetweenPaychecks":
                            return TaxWorld.serverConfig.TimeBetweenPaychecks;
                        case "MoneyCap":
                            return TaxWorld.serverConfig.MoneyCap;
                        case "EnableAutoCollect":
                            return TaxWorld.serverConfig.EnableAutoCollect;
                        case "AddCustomDialog":
                            return TaxWorld.serverConfig.AddCustomDialog;
                        case "ExpertModeBoost":
                            return TaxWorld.serverConfig.ExpertModeBoost;
                        case "IsFlexible":
                            return TaxWorld.serverConfig.IsFlexible;
                    }*/
                    throw new ModSupportException("No config field found by the name of \"" + field + "\"");
                case "GetPaycheck":
                    return ModHandler.parser.CalculateRate();
                case "GetTaxes":
                    LogManager.GetLogger("BetterTaxes").Warn("BetterTaxes.Call() warning: The method \"GetTaxes\" was deprecated in BetterTaxes 2.1.0, as the output is now always equivalent to Player.taxMoney.");
                    if (args.Length < 2) throw new InvalidSyntaxException("GetTaxes <player>");
                    return ((Player)args[1]).taxMoney;
                case "SetTaxes":
                case "ResetPlayerModData": // this alternative way of calling "SetTaxes" is implemented only to follow the guidelines specified by hamstar's Mod Helpers
                    LogManager.GetLogger("BetterTaxes").Warn("BetterTaxes.Call() warning: The method \"" + given_method + "\" was deprecated in BetterTaxes 2.1.0, as the result is now always equivalent to setting Player.taxMoney.");
                    if (args.Length < 2) throw new InvalidSyntaxException(given_method + " <player> [tax amount]");
                    int newAmount = 0;
                    if (args.Length > 2) newAmount = (int)args[2];
                    ((Player)args[1]).taxMoney = newAmount;
                    return true;
            }
            throw new ModSupportException("Unknown method name: " + given_method);
        }
    }
}