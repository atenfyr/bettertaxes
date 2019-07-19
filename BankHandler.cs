using System.Linq;
using Terraria;
using Terraria.ID;

namespace BetterTaxes
{
    public static class BankHandler
    {
        public static bool LastCheckBank = false;
        public static readonly ushort[] SafeTypes = new ushort[] { TileID.PiggyBank, TileID.Safes, TileID.DefendersForge };

        public static int HasBank()
        {
            int collector = NPC.FindFirstNPC(NPCID.TaxCollector);
            if (collector >= 0 && !Main.npc[collector].homeless) return HasBank(Main.npc[collector].homeTileX, Main.npc[collector].homeTileY - 1);
            return -1;
        }

        public static int HasBank(int x, int y)
        {
            if (!WorldGen.StartRoomCheck(x, y)) return -1;
            for (int k = WorldGen.roomY1; k <= WorldGen.roomY2; k++)
            {
                for (int j = WorldGen.roomX1; j <= WorldGen.roomX2; j++)
                {
                    if (Main.tile[j, k] != null && Main.tile[j, k].active())
                    {
                        ushort type = Main.tile[j, k].type;
                        if (SafeTypes.Contains(type)) return type;
                    }
                }
            }

            return -1;
        }

        public static bool CheckIfFull(Chest bank, int type, int amount)
        {
            Item data = new Item();
            data.SetDefaults(type);
            int maxStack = data.maxStack;
            data = null;

            foreach (Item item in bank.item)
            {
                if (item.type == 0 || (item.type == type && item.stack < maxStack)) return false;
            }
            return true;
        }

        public static bool AddItem(Chest bank, int slot, int type, int amount)
        {
            if (amount < 1) return false;
            if (slot < 0) slot += bank.item.Length;
            if (slot > (bank.item.Length - 1)) slot -= bank.item.Length;
            if (CheckIfFull(bank, type, amount)) return false;
            if (bank.item[slot].type == 0)
            {
                bank.item[slot].SetDefaults(type);
                bank.item[slot].stack = amount;
                return true;
            }
            else if (bank.item[slot].type == type)
            {
                if (bank.item[slot].stack + amount >= bank.item[slot].maxStack)
                {
                    int remainder = (bank.item[slot].stack + amount) % 100;

                    int newType = type;
                    int newAmount = bank.item[slot].stack + amount - remainder;
                    if (type > 70 && type < 74)
                    {
                        newType++;
                        newAmount /= 100;
                    }

                    bank.item[slot].stack = remainder;
                    return AddItem(bank, slot + 1, newType, newAmount);
                }
                else
                {
                    bank.item[slot].stack += amount;
                }
            }
            else
            {
                return AddItem(bank, slot + 1, type, amount);
            }
            return true;
        }

        public static int AddCoins(Chest bank, int amount)
        {
            int allCoins = amount;
            int[] coinsArr = Utils.CoinsSplit(amount);
            int type = 71;
            int factor = 1;
            for (int i = 0; i < coinsArr.Length; i++)
            {
                int amn = coinsArr[i];

                if (amn > 0)
                {
                    bool hasSolved = false;
                    for (int j = 0; j < bank.item.Length; j++)
                    {
                        if (bank.item[j].type == type)
                        {
                            if (AddItem(bank, j, type, amn)) allCoins -= amn * factor;
                            hasSolved = true;
                            break;
                        }
                    }

                    if (!hasSolved)
                    {
                        for (int j = 0; j < bank.item.Length; j++)
                        {
                            if (bank.item[j].type == 0)
                            {
                                if (AddItem(bank, j, type, amn)) allCoins -= amn * factor;
                                break;
                            }
                        }
                    }
                }

                type++;
                factor *= 100;
            }
            return allCoins;
        }
    }
}
