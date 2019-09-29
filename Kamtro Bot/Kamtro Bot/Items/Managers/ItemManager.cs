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
        public static Dictionary<uint, Item> Items = null;
        /// <summary>
        /// This method initializes all items to the dictionary, and sets up the shop
        /// </summary>
        public static void SetupItems() {
            Items = new Dictionary<uint, Item>();

            string j = FileManager.ReadFullFile(DataFileNames.ItemMapFile);

            Dictionary<uint, ItemInfoNode> data = JsonConvert.DeserializeObject<Dictionary<uint, ItemInfoNode>>(j);

            foreach(uint k in data.Keys) {
                ItemInfoNode i = data[k];

                Items.Add(k, new Item(k, i.Name, i.Description, i.Rarity, i.Buyable));
            }

            KLog.Info("Loaded Items.");
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