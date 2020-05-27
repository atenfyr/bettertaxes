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
            betterTaxes.Call("AddKey", "YourListName", "YourCondition1", (Func<bool>)delegate(){return YourModWorld.downedYourBoss1;}, Item.buyPrice(0, 10, 0, 0));
            betterTaxes.Call("AddKey", "YourListName", "YourCondition2", (Func<bool>)delegate(){return YourModWorld.downedYourBoss2;}, Item.buyPrice(1, 0, 0, 0));
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
        /// <param name="deleg">A delegate function which returns a boolean corresponding to whether or not the condition is true.</param>
        /// <param name="recommended">The recommended value of this key in copper coins. If this is set to -1, the condition will not be shown to the user.</param>
        /// <returns>A boolean representing whether or not the operation succeeded.</returns>
        public static bool AddKey(string listName, string conditionName, Func<bool> deleg, int recommended) => ModHandler.NewCondition(listName, conditionName, deleg, recommended);
        public static bool AddKey(string listName, string conditionName, Func<bool> deleg) => ModHandler.NewCondition(listName, conditionName, deleg, 0);

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

        [Obsolete("\"AddStatement\" has been replaced with the fourth parameter of \"AddKey\" as of BetterTaxes 2.4.0.")]
        public static bool AddStatement(string statement, int rent) => ModHandler.AddRecommended(statement, rent);

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