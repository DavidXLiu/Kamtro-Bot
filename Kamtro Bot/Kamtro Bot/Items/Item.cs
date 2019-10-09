using Discord;
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
        public ItemRarity Rarity;
        public string Description;
        // public int DefaultSellPrice;
        public int BuyPrice;
        public uint Id;
        public string ImageUrl;

        public bool Buyable;

        private Dictionary<uint, int> Recipe;

        public Item() {
            Id = uint.MaxValue;
            Name = "UNDEFINED";
            Description = "UNDEFINED ITEM. REPORT TO CARBON OR ARCY.";
            BuyPrice = int.MaxValue;
            Rarity = ItemRarity.COMMON;
            ImageUrl = "";
            Recipe = null;
        }

        public Item(uint id, string name, string desc, ItemRarity rarity, bool buyable, int buyPrice, Dictionary<uint, int> recipe = null, string image = "") {
            Id = id;
            Name = name;
            Description = desc;
            Rarity = rarity;
            ImageUrl = image;
            Buyable = buyable;
            BuyPrice = buyPrice;
            Recipe = recipe;
        }

        /// <summary>
        /// Gets the sell price of the item.
        /// </summary>
        /// <returns>The sell price of the item</returns>
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
            return new ItemInfoNode(Name, Description, Rarity, ShopManager.GetAvailability(Id), ShopManager.GetPrice(Id), GetRecipe());
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

        /// <summary>
        /// Retrieves the item's image which should be displayed
        /// </summary>
        /// <returns>The image which should be displayed for the item</returns>
        public string GetImageUrl() {
            if(string.IsNullOrWhiteSpace(ImageUrl)) {
                return ItemManager.DefaultItemImageUrl;
            }

            return ImageUrl;
        }

        public static Color GetColorFromRarity(ItemRarity r) {
            switch(r) {
                case ItemRarity.COMMON:
                    return BotUtils.Grey;

                case ItemRarity.UNCOMMON:
                    return BotUtils.Green;

                case ItemRarity.RARE:
                    return BotUtils.Blue;

                case ItemRarity.EPIC:
                    return BotUtils.Purple;

                case ItemRarity.LEGENDARY:
                    return BotUtils.Orange;

                default:
                    return BotUtils.Kamtro;
            }
        }
    }
}
