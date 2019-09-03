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

        private string Name;
        private ItemRarity Rarity;
        private string Description;
        private int DefaultSellPrice;

        private Dictionary<int, int> Recipe;

        public Item(string name, ItemRarity rarity) {
            Name = name;
            Rarity = rarity;
            Description = desc;


        }


        public int GetSellPrice() {
            // TODO
            return 0;
        }
    }
}
