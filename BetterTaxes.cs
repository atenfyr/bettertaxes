using Terraria.ModLoader;

namespace BetterTaxes {
    class BetterTaxes : Mod {
        public BetterTaxes() {
            Properties = new ModProperties() {
                Autoload = true
            };
        }

        public override void Load() {
            Config.Load();
        }
    }
}