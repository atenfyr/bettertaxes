using System;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;

namespace BetterTaxes
{
    public static class DefaultValues
    {
        public static readonly int taxTimer = 3600;
        public static readonly int taxCap = Item.buyPrice(10, 0, 0, 0);
        public static readonly Dictionary<string, int> taxes = new Dictionary<string, int> {
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
        public static readonly bool addCustomDialog = true;
    }

    public class InvalidConfigException : Exception
    {
        public InvalidConfigException()
        {
        }

        public InvalidConfigException(string message) : base("Malformed config: " + message)
        {
        }

        public InvalidConfigException(string message, Exception inner) : base("Malformed config: " + message, inner)
        {
        }
    }

    public static class Config
    {
        private static readonly string path = Path.Combine(Main.SavePath, "Mod Configs", "BetterTaxes.json");
        private static Preferences config = new Preferences(path);

        private static void CreateConfig()
        {
            config.Clear();
            config.Put("TaxRates", DefaultValues.taxes);
            config.Put("TimeBetweenPaychecks", DefaultValues.taxTimer / 60);
            config.Put("MoneyCap", DefaultValues.taxCap);
            config.Put("AddCustomDialog", DefaultValues.addCustomDialog);
            config.Save();

            TaxWorld.taxes = DefaultValues.taxes;
            TaxWorld.taxTimer = DefaultValues.taxTimer;
            TaxWorld.taxCap = DefaultValues.taxCap;
            TaxWorld.addCustomDialog = DefaultValues.addCustomDialog;
        }

        public static void Load()
        {
            if (config.Load()) 
            {
                if (!config.Contains("TaxRates"))
                {
                    throw new InvalidConfigException("Config is missing TaxRates field.");
                }
                if (!config.Contains("TimeBetweenPaychecks"))
                {
                    throw new InvalidConfigException("Config is missing TimeBetweenPaychecks field.");
                }
                if (!config.Contains("MoneyCap"))
                {
                    throw new InvalidConfigException("Config is missing MoneyCap field.");
                }
                if (!config.Contains("AddCustomDialog"))
                {
                    throw new InvalidConfigException("Config is missing AddCustomDialog field.");
                }

                config.Get("TaxRates", ref TaxWorld.taxes);
                config.Get("TimeBetweenPaychecks", ref TaxWorld.taxTimer);
                config.Get("MoneyCap", ref TaxWorld.taxCap);
                config.Get("AddCustomDialog", ref TaxWorld.addCustomDialog);
                TaxWorld.taxTimer *= 60; // 60 frames are in a second
                if (TaxWorld.taxTimer < 1) // minimum is 1 frame
                {
                    TaxWorld.taxTimer = DefaultValues.taxTimer;
                }
                if (TaxWorld.taxCap < 1) // minimum is 1 copper
                {
                    TaxWorld.taxCap = DefaultValues.taxCap;
                }

                foreach (KeyValuePair<string, int> entry in TaxWorld.taxes)
                {
                    if (entry.Value < 0)
                    {
                        throw new InvalidConfigException("Cannot have invalid tax value \"" + entry.Value + "\" in the flag \"" + entry.Key + "\".");
                    }
                }

                // we run through the config once to make sure it's not malformed
                foreach (KeyValuePair<string, int> entry in TaxWorld.taxes)
                {
                    GateParser.Interpret(entry.Key);
                }
            }
            else
            {
                CreateConfig();
            }
        }
    }
}