using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;

namespace BetterTaxes
{
    public static class DefaultValues
    {
        public static readonly int taxTimer = 3600;
        public static readonly int taxCap = Item.buyPrice(platinum: 10);
        public static readonly Dictionary<string, int> taxes = new Dictionary<string, int> {
            {"PostWall", Item.buyPrice(copper: 50)},
            {"PostAnyMechBoss", Item.buyPrice(silver: 1)},
            {"PostPlantera", Item.buyPrice(silver: 2)},
            {"PostGolem", Item.buyPrice(silver: 5)},
            {"PostMoonLord", Item.buyPrice(silver: 10)}
        };
        public static readonly bool addCustomDialog = true;
    }

    public static class Config
    {
        private static readonly string path = Path.Combine(Main.SavePath, "Mod Configs", "BetterTaxes.json");
        private static Preferences config = new Preferences(path);

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
                if (TaxWorld.taxes.ContainsKey("PostWall") && TaxWorld.taxes.ContainsKey("PostAnyMechBoss") && TaxWorld.taxes.ContainsKey("PostPlantera") && TaxWorld.taxes.ContainsKey("PostGolem") && TaxWorld.taxes.ContainsKey("PostMoonLord"))
                {
                    foreach (KeyValuePair<string, int> entry in TaxWorld.taxes)
                    {
                        if (entry.Value < 1)
                        {
                            TaxWorld.taxes = DefaultValues.taxes;
                            break;
                        }
                    }
                }
                else
                {
                    TaxWorld.taxes = DefaultValues.taxes;
                }
            }
            else
            {
                ErrorLogger.Log("Making a config for Better Taxes");
                config.Clear();
                config.Put("TaxRates", DefaultValues.taxes);
                config.Put("TimeBetweenPaychecks", (DefaultValues.taxTimer) / 60);
                config.Put("MoneyCap", DefaultValues.taxCap);
                config.Put("AddCustomDialog", DefaultValues.addCustomDialog);
                config.Save();
            }
        }
    }
}