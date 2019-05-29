using System;

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

        /*
            Mod betterTaxes = ModLoader.GetMod("BetterTaxes");
		    if (betterTaxes != null)
            {
                // Adding a brand new list
                betterTaxes.Call("AddList", "YourListName", (Func<string, bool>)delegate(string name)
				{
					switch (name)
                    {
                        case "YourCondition1":
                            return YourModWorld.downedYourBoss1;
                        case "YourCondition2":
                            return YourModWorld.downedYourBoss2;
                        default:
                            return false;
                    }
				});

                // Adding a statement to flexible config files
                betterTaxes.Call("AddStatement", "YourListName.YourCondition1", 9999);
                betterTaxes.Call("Save");
            }
        */

        public static object Call(params object[] args)
        {
            if (args.Length <= 0 || !(args[0] is string)) return new ModSupportException("The first parameter must be a method name");
            string given_method = (string)args[0];
            switch (given_method)
            {
                case "AddList":
                    if (args.Length < 3 || !(args[1] is string) || !(args[2] is Func<string, bool>)) return new ModSupportException("Usage: AddList <list name> <function taking conditions as a string and returning a boolean>");
                    string key = (string)args[1];
                    if (TaxConstants.delegates.ContainsKey(key)) TaxConstants.delegates.Remove(key);
                    TaxConstants.delegates.Add(key, (Func<string, bool>)args[2]);
                    return true;
                case "AddStatement":
                    if (args.Length < 3 || !(args[1] is string) || !(args[2] is int)) return new ModSupportException("Usage: AddStatement <statement> <rent in copper coins>");
                    return Config.AddStatement((string)args[1], (int)args[2]);
                case "Save":
                    Config.Save();
                    return true;
                default:
                    return new ModSupportException("No method found by the name of " + given_method);
            }
        }
    }
}
