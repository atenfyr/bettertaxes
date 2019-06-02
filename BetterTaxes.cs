using Terraria.ModLoader;

namespace BetterTaxes
{
    class BetterTaxes : Mod
    {
        public static string GithubUserName => "atenfyr";
        public static string GithubProjectName => "bettertaxes";

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
            new ModHandler(); // resets all the delegates etc
        }

        public override void Unload()
        {
            TaxWorld.serverConfig = null;
        }
    }
}