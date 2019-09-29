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

        public string Name;
        private ItemRarity Rarity;
        private string Description;
        private int DefaultSellPrice;
        public int BuyPrice;
        private uint Id;
        private string ImageUrl;

        public bool Buyable;

        private Dictionary<uint, int> Recipe = null;

        public Item(uint id, string name, string desc, ItemRarity rarity, bool buyable, int buyPrice = 0, string image = "") {
            Id = id;
            Name = name;
            Description = desc;
            Rarity = rarity;
            ImageUrl = image;
            Buyable = buyable;
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

            foreach (uint i in Recipe.Keys) {
                total += ItemManager.GetItem(i).GetSellPrice() * Recipe[i];
            }

            return total;
        }

        public ItemInfoNode GetItemInfo() {
            return new ItemInfoNode(Name, Description, Rarity, ShopManager.GetAvailability(Id), ShopManager.GetPrice(Id));
        }

        public Dictionary<uint, int> GetRecipe() {
            return Recipe;
        }

        /// <summary>
        /// Tells if the item has a recipe and is craftable
        /// </summary>
        /// <returns>True if the item is craftable, false otherwise.</returns>
        public bool IsCraftable() {
            return Recipe == null || Recipe.Count <= 0;
        }
    }
}
