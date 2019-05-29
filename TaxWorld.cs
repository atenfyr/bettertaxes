using System.IO;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace BetterTaxes
{
    public class TaxWorld : ModWorld
    {
        public static int taxTimer = DefaultValues.taxTimer;
        public static int taxCap = DefaultValues.taxCap;
        public static Dictionary<string, int> taxes = DefaultValues.taxes;

        // when we first load the world, it's our personal config. if and when we sync, that'll overwrite the old data and put in the new if necessary
        public override void Initialize()
        {
            taxTimer = Config.taxTimer;
            taxCap = Config.taxCap;
            taxes = Config.taxes;
        }

        // sync config from server to client. we intentionally do not sync addCustomDialog because that should be the user's choice, not the server's
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Config.taxTimer);
            writer.Write(Config.taxCap);
            writer.Write(Config.taxes.Keys.Count);
            foreach (var item in Config.taxes)
            {
                writer.Write(item.Key);
                writer.Write(item.Value);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            taxTimer = reader.ReadInt32();
            taxCap = reader.ReadInt32();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string progression = reader.ReadString();
                int rent = reader.ReadInt32();
                taxes[progression] = rent;
            }
        }
    }
}
