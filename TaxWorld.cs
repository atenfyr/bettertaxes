using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace BetterTaxes
{
    public class TaxWorld : ModWorld
    {
        public static BetterTaxesConfig serverConfig;
        private bool hasSynced = false;

        public static bool[] ClientBanksList = new bool[BankHandler.SafeTypes.Length];
        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            for (int i = 0; i < BankHandler.SafeTypes.Length; i++)
            {
                flags[i] = ClientBanksList[i];
            }
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            for (int i = 0; i < BankHandler.SafeTypes.Length; i++)
            {
                ClientBanksList[i] = flags[i];
            }
        }

        public override void PostUpdate()
        {
            if (Main.netMode == 1) return;

            if (Main.dayTime && hasSynced) hasSynced = false;
            if (serverConfig.EnableAutoCollect && !hasSynced && !Main.dayTime && Main.time >= 15000 && Main.time < 16200) // 20-second updating window
            {
                ClientBanksList = BankHandler.HasBank();
                hasSynced = true;
            }
        }
    }
}