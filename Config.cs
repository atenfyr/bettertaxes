using System.IO;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;

namespace BetterTaxes {
    public static class Config {
        private static readonly string path = Path.Combine(Main.SavePath, "Mod Configs", "BetterTaxes.json");
        private static Preferences config = new Preferences(path);

        public static void Load() {
            if (config.Load()) {
                config.Get("TaxRates", ref TaxWorld.taxes);
                config.Get("TimeBetweenPaychecks", ref TaxWorld.taxTimer);
                config.Get("MoneyCap", ref TaxWorld.taxCap);
                TaxWorld.taxTimer *= 60; // 60 frames are in a second
                if (TaxWorld.taxTimer < 1) {
                    TaxWorld.taxTimer = 1;
                }
                if (TaxWorld.taxCap < 1) {
                    TaxWorld.taxCap = 1;
                }
            } else {
                ErrorLogger.Log("Making a config for Better Taxes");
                config.Clear();
                config.Put("TaxRates", TaxWorld.taxes);
                config.Put("TimeBetweenPaychecks", (TaxWorld.taxTimer)/60);
                config.Put("MoneyCap", TaxWorld.taxCap);
                config.Save();
            }
        }
    }
}