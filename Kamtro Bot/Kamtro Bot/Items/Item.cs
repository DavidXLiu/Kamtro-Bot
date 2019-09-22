using Kamtro_Bot.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Items
{
    public class Item {
        #region Enums
        public enum ItemRarity {
            COMMON,
            UNCOMMON,
            RARE,
            EPIC,
            LEGENDARY
        }
        #endregion

        public const double BUY_TO_SELL_MULTIPLIER = 0.75;

        private string Name;
        private ItemRarity Rarity;
        private string Description;
        private int DefaultSellPrice;
        private uint Id;
        private string ImageUrl;

        private Dictionary<int, int> Recipe = null;

        public Item(uint id, string name, string desc, ItemRarity rarity, string image = "") {
            Id = id;
            Name = name;
            Description = desc;
            Rarity = rarity;
            ImageUrl = image;
        }
         
        /// <summary>
        /// Gets the sell price of the item.
        /// </summary>
        /// <returns></returns>
        public int GetSellPrice() {
            // Base Case
            if (Recipe == null) {
                return ShopManager.GetPrice(Id);
            }

            int total = 0;

            foreach(int i in Recipe.Values) {
                total += GetItem(i).GetSellPrice();
            }

            return total;
        }

        public ItemInfoNode GetItemInfo() {
            return new ItemInfoNode(Name, Description, Rarity, ShopManager.GetAvailability(Id), ShopManager.GetPrice(Id));
        }

        // TODO
        private Item GetItem(uint i) {
            return null;
        }
    }
}
