﻿using System;

namespace BetterTaxes
{
    public class ModSupport
    {
        public class ModSupportException : Exception
        {
            public static readonly string messageFormat = "Mod support error from BetterTaxes: {0}.";
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
            if (args.Length <= 0 || !(args[0] is string)) return new ModSupportException("The first parameter must be a method name");
            string given_method = (string)args[0];
            switch (given_method)
            {
                case "AddList":
                    if (args.Length < 3 || !(args[1] is string)) return new ModSupportException("Usage: AddList <list name>");
                    ModHandler.NewList((string)args[1]);
                    return true;
                case "AddKey":
                    if (args.Length < 3 || !(args[1] is string) || !(args[2] is string) || !(args[3] is Func<bool>)) return new ModSupportException("Usage: AddKey <list name> <condition name> <function returning a boolean>");
                    ModHandler.NewCondition((string)args[1], (string)args[2], (Func<bool>)args[3]);
                    return true;
                case "AddStatement":
                    if (args.Length < 3 || !(args[1] is string) || !(args[2] is int)) return new ModSupportException("Usage: AddStatement <statement> <rent in copper coins>");
                    return Config.AddStatement((string)args[1], (int)args[2]);
                case "Save": // you shouldn't typically call this unless you have a good reason
                    return Config.Save();
            }
            return new ModSupportException("No method found by the name of " + given_method);
        }
    }
}