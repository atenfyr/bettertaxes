using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria.ModLoader.Config;
using BetterTaxes.UI;

namespace BetterTaxes
{
    public class InvalidConfigException : Exception
    {
        public static readonly string messageFormat = "Malformed config: {0}. See https://github.com/atenfyr/bettertaxes/blob/master/CONFIG.md for more information.";
        public InvalidConfigException()
        {
        }

        public InvalidConfigException(string message) : base(string.Format(messageFormat, message))
        {
        }

        public InvalidConfigException(string message, Exception inner) : base(string.Format(messageFormat, message), inner)
        {
        }
    }

    public struct StaticConstants
    {
        public static readonly Dictionary<string, int> TaxRatesDefaults = new Dictionary<string, int> {
            {"Base.always", 50},
            {"Base.mechAny", 100},
            {"Base.plantera", 200},
            {"Base.golem", 500},
            {"Base.moonlord", 1000},
            {"Calamity.providence", 1250},
            {"Thorium.ragnarok", 1500},
            {"Calamity.dog", 1500},
            {"Calamity.yharon", 2500},
            {"Calamity.scal", 5000},
        };
    }

    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Tooltip("Maps \"statements\" representing game progression to rent values, represented in copper coins. See the GitHub page.")]
        public Dictionary<string, int> TaxRates;

        [Tooltip("The amount of time between updates of the Tax Collector's money storage.")]
        [DefaultValue(60)]
        [SliderColor(183, 88, 25)]
        [CustomModConfigItem(typeof(TimeRangeElement))]
        public int TimeBetweenPaychecks;

        [Label("MoneyCap")]
        [Tooltip("The amount of money that the Tax Collector can hold at once.")]
        [DefaultValue(10000000)]
        [SliderColor(204, 181, 72)]
        [CustomModConfigItem(typeof(CoinRangeElement))]
        public int MoneyCap;

        [Tooltip("Should the new lines of dialog be added to the Tax Collector's dialog pool?")]
        [DefaultValue(true)]
        public bool AddCustomDialog;

        [Tooltip("Should this config file be subject to automatic changes made by this mod and other mods?")]
        [DefaultValue(true)]
        public bool IsFlexible;

        public override ModConfig Clone()
        {
            var clone = (Config)base.Clone();
            clone.TaxRates = TaxRates?.ToDictionary(i => i.Key, i => i.Value);
            return clone;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            TaxRates = TaxRates?.Count > 0 ? TaxRates : StaticConstants.TaxRatesDefaults.ToDictionary(i => i.Key, i => i.Value);
            TimeBetweenPaychecks = TimeBetweenPaychecks > 0 ? TimeBetweenPaychecks : 60;
        }

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
        {
            var thisConfig = (Config)pendingConfig;
            if (thisConfig.TaxRates.Count == TaxRates.Count && !thisConfig.TaxRates.Except(TaxRates).Any() && thisConfig.TimeBetweenPaychecks == TimeBetweenPaychecks && thisConfig.MoneyCap == MoneyCap && thisConfig.IsFlexible == IsFlexible) return true;
            message = "Only AddCustomDialog can be changed while connected to a server";
            return false;
        }

        public override void OnLoaded()
        {
            TaxWorld.serverConfig = this;
        }

        /*public override void OnChanged()
        {
            if (IsFlexible)
            {
                foreach (KeyValuePair<string, int> entry in StaticConstants.TaxRatesDefaults)
                {
                    if (!TaxRates.ContainsKey(entry.Key))
                    {
                        AddStatement(entry.Key, entry.Value);
                    }
                }
            }
        }*/

        public bool AddStatement(string statement, int value)
        {
            if (!IsFlexible) return false;
            if (TaxRates.ContainsKey(statement)) TaxRates.Remove(statement);
            TaxRates.Add(statement, value);
            return true;
        }
    }
}