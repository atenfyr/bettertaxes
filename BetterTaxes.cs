using Terraria.ModLoader;

namespace BetterTaxes
{
    class BetterTaxes : Mod
    {
        public BetterTaxes()
        {
            Properties = new ModProperties()
            {
                Autoload = true
            };
        }

        public override object Call(params object[] args)
        {
            return ModSupport.Call(args);
        }

        public override void Load()
        {
            Config.Load();
        }
    }
}