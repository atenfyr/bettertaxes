using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BetterTaxes {
    public class TaxWorld : ModWorld {
        public static int taxTimer = 3600;
        public static int taxCap = Item.buyPrice(platinum: 10);
        public static Dictionary<string, int> taxes = new Dictionary<string, int> {
            {"PostWall", Item.buyPrice(copper: 50)},
            {"PostAnyMechBoss", Item.buyPrice(silver: 1)},
            {"PostPlantera", Item.buyPrice(silver: 2)},
            {"PostGolem", Item.buyPrice(silver: 5)},
            {"PostMoonLord", Item.buyPrice(silver: 10)}
        };

        // sync config from server to client
        public override void NetSend(BinaryWriter writer) {
            writer.Write(taxTimer);
            writer.Write(taxCap);
            writer.Write(taxes.Keys.Count);
            foreach (var item in taxes) {
                writer.Write(item.Key);
                writer.Write(item.Value);
            }
        }

        public override void NetReceive(BinaryReader reader) {
            taxTimer = reader.ReadInt32();
            taxCap = reader.ReadInt32();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++) {
                string progression = reader.ReadString();
                int rent = reader.ReadInt32();
                taxes[progression] = rent;
            }
        }
    }
}
