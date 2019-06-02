using System;
using System.Linq;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Terraria.ModLoader.Config;

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

    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        private static readonly Dictionary<string, int> TaxRatesDefaults = new Dictionary<string, int> {
            {"Base.always", 50},
            {"Base.downedMechBossAny", 100},
            {"Base.downedPlantBoss", 200},
            {"Base.downedGolemBoss", 500},
            {"Base.downedMoonlord", 1000},
            {"Calamity.downedProvidence", 1250},
            {"Thorium.downedRealityBreaker", 1500},
            {"Calamity.downedDoG", 1500},
            {"Calamity.downedYharon", 2500},
            {"Calamity.downedSCal", 5000},
        };

        [Tooltip("Maps \"statements\" to rent values, represented in copper coins. See the GitHub page.")]
        public Dictionary<string, int> TaxRates;
        [Tooltip("The amount of time in seconds between updates of the Tax Collector's money storage.")]
        [DefaultValue(60)]
        public int TimeBetweenPaychecks;
        [Tooltip("The amount of money represented in copper coins that the Tax Collector can hold.")]
        [DefaultValue(10000000)]
        public int MoneyCap;
        [Tooltip("A boolean (true or false) which corresponds to whether or not the new lines of dialog should be added to the Tax Collector's dialog pool.")]
        [DefaultValue(true)]
        public bool AddCustomDialog;
        [Tooltip("A boolean (true or false) which corresponds to whether or not this config file is subject to automatic changes made by this mod and other mods. You should set this to true if you are making changes to the config.")]
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
            TaxRates = TaxRates?.Count == TaxRatesDefaults.Count ? TaxRates : TaxRatesDefaults.ToDictionary(i => i.Key, i => i.Value);
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

        public override void OnChanged()
        {
            if (TimeBetweenPaychecks < 1) throw new InvalidConfigException("Tax timer value \"" + TimeBetweenPaychecks / 60 + "\" is too small");
            if (MoneyCap < 1) throw new InvalidConfigException("Tax cap value \"" + MoneyCap + "\" is too small");

            foreach (KeyValuePair<string, int> entry in TaxRates)
            {
                if (entry.Value < 0) throw new InvalidConfigException("Tax value \"" + entry.Value + "\" in the flag \"" + entry.Key + "\" is too small");
            }

            if (IsFlexible)
            {
                foreach (KeyValuePair<string, int> entry in TaxRatesDefaults)
                {
                    if (!TaxRates.ContainsKey(entry.Key))
                    {
                        AddStatement(entry.Key, entry.Value);
                    }
                }
            }
        }

        public bool AddStatement(string statement, int value)
        {
            if (!IsFlexible) return false;
            if (TaxRates.ContainsKey(statement)) TaxRates.Remove(statement);
            TaxRates.Add(statement, value);
            return true;
        }
    }
}