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
        public static readonly int fieldCount = 4;
    }

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

    public static class Config
    {
        public static int taxTimer = DefaultValues.taxTimer;
        public static int taxCap = DefaultValues.taxCap;
        public static Dictionary<string, int> taxes = DefaultValues.taxes;
        public static bool addCustomDialog = true;

        private static string path = Path.Combine(Main.SavePath, "Mod Configs", "BetterTaxes.json");
        private static Preferences config = new Preferences(path);

        private static void CreateConfig()
        {
            config.Clear();
            config.Put("TaxRates", DefaultValues.taxes);
            config.Put("TimeBetweenPaychecks", DefaultValues.taxTimer / 60);
            config.Put("MoneyCap", DefaultValues.taxCap);
            config.Put("AddCustomDialog", DefaultValues.addCustomDialog);
            config.Save();

            taxes = DefaultValues.taxes;
            taxTimer = DefaultValues.taxTimer;
            taxCap = DefaultValues.taxCap;
            addCustomDialog = DefaultValues.addCustomDialog;
        }

        public static void Load()
        {
            if (config.Load())
            {
                int currentFieldCount = config.GetAllKeys().Count;
                if (currentFieldCount != DefaultValues.fieldCount)
                {
                    if (currentFieldCount > DefaultValues.fieldCount) throw new InvalidConfigException("Config contains " + (currentFieldCount - DefaultValues.fieldCount) + " extraneous field(s)");
                    if (currentFieldCount < DefaultValues.fieldCount) throw new InvalidConfigException("Config is missing " + (DefaultValues.fieldCount - currentFieldCount) + " field(s)");
                }

                config.Get("TaxRates", ref taxes);
                config.Get("TimeBetweenPaychecks", ref taxTimer);
                config.Get("MoneyCap", ref taxCap);
                config.Get("AddCustomDialog", ref addCustomDialog);

                if (taxes.ContainsKey("PostWall")) // pre-1.0.0 config file, we need to do a full reset
                {
                    CreateConfig();
                }
                else
                {
                    taxTimer *= 60; // 60 frames are in a second
                    if (taxTimer < 1) throw new InvalidConfigException("Tax timer value \"" + taxTimer / 60 + "\" is too small");
                    if (taxCap < 1) throw new InvalidConfigException("Tax cap value \"" + taxCap + "\" is too small");

                    foreach (KeyValuePair<string, int> entry in taxes)
                    {
                        if (entry.Value < 0)
                        {
                            throw new InvalidConfigException("Tax value \"" + entry.Value + "\" in the flag \"" + entry.Key + "\" is too small");
                        }
                    }
                }
            }
            else
            {
                CreateConfig();
            }

            TaxWorld.taxes = taxes;
            TaxWorld.taxTimer = taxTimer;
            TaxWorld.taxCap = taxCap;
        }
    }
}