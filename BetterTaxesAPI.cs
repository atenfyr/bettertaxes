using System;
using System.Reflection;

namespace BetterTaxes
{
    // Sample snippet:
    /*
        Mod betterTaxes = ModLoader.GetMod("BetterTaxes");
        if (betterTaxes != null)
        {
            // Adding a brand new list with conditions
            betterTaxes.Call("AddList", "YourListName"); // redundant
            betterTaxes.Call("AddKey", "YourListName", "YourCondition1", (Func<bool>)delegate(){return YourModWorld.downedYourBoss1;});
            betterTaxes.Call("AddKey", "YourListName", "YourCondition2", (Func<bool>)delegate(){return YourModWorld.downedYourBoss2;});
    
            // Adding a statement to flexible config files
            betterTaxes.Call("AddStatement", "YourListName.YourCondition1", 9999);
        }
    */

    public static class BetterTaxesAPI
    {
        /// <summary>
        /// Creates a new list.
        /// </summary>
        /// <param name="listName">The name of the list.</param>
        /// <returns>A boolean representing whether or not the operation succeeded.</returns>
        public static bool AddList(string listName) => ModHandler.NewList(listName);

        /// <summary>
        /// Creates a new key.
        /// </summary>
        /// <param name="listName">The name of the list.</param>
        /// <param name="conditionName">The name of the condition.</param>
        /// <param name="deleg">A delegate function which returns a boolean corresponding to whether or not condition is true.</param>
        /// <returns>A boolean representing whether or not the operation succeeded.</returns>
        public static bool AddKey(string listName, string conditionName, Func<bool> deleg) => ModHandler.NewCondition(listName, conditionName, deleg);

        /// <summary>
        /// Creates a new statement in the config file if its IsFlexible field is set to true.
        /// </summary>
        /// <param name="statement">The statement to add to the config file.</param>
        /// <param name="rent">The amount of amount of money per NPC represented in copper coins to give per minute if the given statement is true.</param>
        /// <returns>A boolean representing whether or not the operation succeeded.</returns>
        public static bool AddStatement(string statement, int rent) => ModHandler.AddStatement(statement, rent);

        /// <summary>
        /// Gets the value of a requested field in the config file.
        /// </summary>
        /// <param name="field">The field in the config file to retrieve.</param>
        /// <returns>An object equal to the field requested in the config file.</returns>
        public static object GetConfig(string field)
        {
            object data = null;
            PropertyInfo info = TaxWorld.serverConfig.GetType().GetProperty(field);
            if (info != null) data = info.GetValue(TaxWorld.serverConfig);

            if (data != null)
            {
                if (data is SpecialInt) return (int)(SpecialInt)data;
                return data;
            }
            throw new ModSupportException("No config field found by the name of \"" + field + "\"");
        }

        /// <summary>
        /// Gets the current cost of rent.
        /// </summary>
        /// <returns>An integer equal to the amount of money per NPC represented in copper coins being given per minute.</returns>
        public static int GetPaycheck() => ModHandler.parser.CalculateRate();

        [Obsolete("The method \"Save\" was removed in BetterTaxes 2.0.0 due to the limitations of tModLoader v0.11's ModConfig class.")]
        public static bool Save() => false;
    }

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
}