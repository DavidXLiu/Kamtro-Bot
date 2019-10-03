using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kamtro_Bot.Items.Item;

namespace Kamtro_Bot.Items
{
    public class ItemInfoNode
    {
        public string Name;
        public string Description;
        public string ImageUrl;
        public int BuyPrice;
        public bool Buyable;  // If the item is available in the shop
        public ItemRarity Rarity;

        public Dictionary<uint, int> Recipe;

        public ItemInfoNode(string name = "Unnamed Item", string desc = "Report to carbon or arcy.", ItemRarity rarity = ItemRarity.COMMON, bool avail = false, int price = 0, Dictionary<uint, int> recipe = null, string image = "") {
            Name = name;
            Description = desc;
            Rarity = rarity;
            Buyable = avail;
            BuyPrice = price;
            ImageUrl = image;

            Recipe = recipe;
        }
    }
}
