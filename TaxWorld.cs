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
        public static bool addCustomDialog = DefaultValues.addCustomDialog;

        // sync config from server to client
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(taxTimer);
            writer.Write(taxCap);
            writer.Write(taxes.Keys.Count);
            foreach (var item in taxes)
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
