using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Items
{
    public class ItemManager
    {
        public const string DefaultItemImageUrl = "https://cdn.discordapp.com/attachments/631239436365070372/631239508435795998/New_Piskel.png";

        public static Dictionary<uint, Item> Items = null;
        
        /// <summary>
        /// This method initializes all items to the dictionary, and sets up the shop
        /// </summary>
        public static void SetupItems() {
            Items = new Dictionary<uint, Item>();

            string j = FileManager.ReadFullFile(DataFileNames.ItemMapFile);

            Dictionary<uint, ItemInfoNode> data = JsonConvert.DeserializeObject<Dictionary<uint, ItemInfoNode>>(j) ?? new Dictionary<uint, ItemInfoNode>();

            AddSpecialItems();

            foreach(uint k in data.Keys) {
                ItemInfoNode i = data[k];

                if(Items.ContainsKey(k)) {
                    Item it = Items[k];

                    it.Id = k;
                    it.Name = i.Name;
                    it.Description = i.Description;
                    it.Rarity = i.Rarity;
                    it.BuyPrice = i.BuyPrice;
                    it.Buyable = i.Buyable;
                    it.ImageUrl = i.ImageUrl;
                    it.SetRecipe(i.Recipe);
                    continue;
                }

                Items.Add(k, new Item(k, i.Name, i.Description, i.Rarity, i.Buyable, i.BuyPrice, i.Recipe, i.ImageUrl));
            }

            KLog.Info("Loaded Items.");
        }

        private static void AddSpecialItems() {
            // Add all item classes
            Items.Add(5, new ItemBreadstick());
            Items.Add(8, new ItemPizza());
            Items.Add(12, new ItemPasta());
            Items.Add(15, new ItemPokePuff());
        }

        public static void SaveItemData() {
            Dictionary<uint, ItemInfoNode> data = new Dictionary<uint, ItemInfoNode>();
            
            foreach(uint i in Items.Keys) {
                data[i] = Items[i].GetItemInfo();
            }

            BotUtils.WriteToJson(data, DataFileNames.ItemMapFile);
        }

        public static bool IsBuyable(uint id) {
            return Items[id].Buyable;
        }

        public static Item GetItem(uint id) {
            if(Items == null) {
                SetupItems();
            }

            Item ret = null;

            if(Items.ContainsKey(id)) {
                ret = Items[id];
            }

            return ret;
        }  
    }
}