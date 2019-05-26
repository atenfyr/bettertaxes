using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;

namespace BetterTaxes
{
    public static class DefaultValues
    {
        public static readonly int taxTimer = 3600;
        public static readonly int taxCap = 10000000;
        public static readonly Dictionary<string, int> taxes = new Dictionary<string, int> {
            {"Base.always", 50},
            {"Base.downedMechBossAny", 100},
            {"Base.downedPlantBoss", 200},
            {"Base.downedGolemBoss", 500},
            {"Base.downedMoonlord", 1000},
            {"Calamity.downedProvidence", 1250},
            {"ThoriumMod.ThoriumWorld.downedRealityBreaker", 1500},
            {"Calamity.downedDoG", 1500},
            {"Calamity.downedYharon", 2500},
            {"Calamity.downedSCal", 5000},
        };
        public static readonly bool addCustomDialog = true;
    }

    public static class Config
    {
        private static readonly string path = Path.Combine(Main.SavePath, "Mod Configs", "BetterTaxes.json");
        private static Preferences config = new Preferences(path);

        private static void CreateConfig()
        {
            config.Clear();
            config.Put("TaxRates", DefaultValues.taxes);
            config.Put("TimeBetweenPaychecks", (DefaultValues.taxTimer) / 60);
            config.Put("MoneyCap", DefaultValues.taxCap);
            config.Put("AddCustomDialog", DefaultValues.addCustomDialog);
            config.Save();
        }

        public static void Load()
        {
            if (config.Load()) 
            {
                config.Get("TaxRates", ref TaxWorld.taxes);
                config.Get("TimeBetweenPaychecks", ref TaxWorld.taxTimer);
                config.Get("MoneyCap", ref TaxWorld.taxCap);
                config.Get("AddCustomDialog", ref TaxWorld.addCustomDialog);
                TaxWorld.taxTimer *= 60; // 60 frames are in a second
                if (TaxWorld.taxTimer < 1)
                { // minimum is 1 frame
                    TaxWorld.taxTimer = DefaultValues.taxTimer;
                }
                if (TaxWorld.taxCap < 1)
                { // minimum is 1 copper
                    TaxWorld.taxCap = DefaultValues.taxCap;
                }

                foreach (KeyValuePair<string, int> entry in TaxWorld.taxes)
                {
                    if (entry.Value < 1)
                    {
                        TaxWorld.taxes = DefaultValues.taxes;
                        break;
                    }
                }

                bool hasChanged = false;
                foreach (KeyValuePair<string, int> entry in DefaultValues.taxes)
                {
                    if (!TaxWorld.taxes.ContainsKey(entry.Key))
                    {
                        TaxWorld.taxes.Add(entry.Key, entry.Value);
                        hasChanged = true;
                    }
                }
                if (!config.Contains("TimeBetweenPaychecks"))
                {
                    TaxWorld.taxTimer = DefaultValues.taxTimer;
                    hasChanged = true;
                }
                if (!config.Contains("MoneyCap"))
                {
                    TaxWorld.taxCap = DefaultValues.taxCap;
                    hasChanged = true;
                }
                if (!config.Contains("AddCustomDialog"))
                {
                    TaxWorld.addCustomDialog = DefaultValues.addCustomDialog;
                    hasChanged = true;
                }
                if (hasChanged)
                {
                    config.Clear();
                    config.Put("TaxRates", TaxWorld.taxes);
                    config.Put("TimeBetweenPaychecks", (TaxWorld.taxTimer) / 60);
                    config.Put("MoneyCap", TaxWorld.taxCap);
                    config.Put("AddCustomDialog", TaxWorld.addCustomDialog);
                    config.Save();
                }
            }
            else
            {
                CreateConfig();
            }
        }
    }
}