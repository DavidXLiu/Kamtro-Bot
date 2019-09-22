using Kamtro_Bot.Items;
using Kamtro_Bot.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Items
{
    public class ShopManager
    {
        public static Dictionary<uint, ShopNode> Shop = new Dictionary<uint, ShopNode>();

        public static void AddItem(uint id, int price, bool avail) {
            ShopNode sn = new ShopNode(price, avail);

            if (Shop.ContainsKey(id)) {
                KLog.Info($"Replaced item in shop at id {id}");
            }

            Shop[id] = sn;
        }

        /// <summary>
        /// Refreshes the shop's selection
        /// </summary>
        /// <returns>The new selection of items</returns>
        public static List<uint> GenShopSelection() {
            Shop.Clear();

            List<uint> final = new List<uint>();
            List<uint> options = GetSellableItems();

            Random r = new Random();
            int n;
            for(int i = 0; i < 5; i++) {
                n = r.Next(0, options.Count);
                final.Add(options[n]);
            }

            foreach(uint i in final) {
                Shop.Add(i, new ShopNode(ItemManager.Items[i].GetSellPrice(), ItemManager.Items[i].Buyable));
            }

            KLog.Info("Shop Refreshed.");

            return final;
        }

        public static List<uint> ValidateShopSelection() {
            List<uint> toReplace = new List<uint>();
            List<uint> replaced = new List<uint>();

            foreach(uint i in Shop.Keys) {
                if(!ItemManager.IsBuyable(i)) {
                    toReplace.Add(i);
                }
            }

            foreach(uint i in toReplace) {
                Shop.Remove(i);

                Tuple<uint, ShopNode> t = GetRandomNewSellableItem();

                Shop.Add(t.Item1, t.Item2);
                replaced.Add(t.Item1);
            }

            return replaced;
        }

        public static void AddItem(uint id, ItemInfoNode i) {
            AddItem(id, i.BuyPrice, i.Buyable);
        }

        public static int GetPrice(uint itemid) {
            return Shop[itemid].Price;
        }

        public static bool GetAvailability(uint itemid) {
            return Shop[itemid].Available;
        }

        public static void SetAvailability(uint id, bool a) {
            KLog.Info($"{(a ? "Enabled":"Disabled")} the item at id {id} in the shop");

            Shop[id].Available = a;
            ValidateShopSelection();
        }

        private static List<uint> GetSellableItems() {
            List<uint> options = new List<uint>();

            foreach (uint k in ItemManager.Items.Keys) {
                if (ItemManager.Items[k].Buyable) options.Add(k);
            }

            return options;
        }

        private static Tuple<uint, ShopNode> GetRandomNewSellableItem() {
            List<uint> tmp = new List<uint>();

            foreach(uint i in GetSellableItems()) {
                if (!Shop.ContainsKey(i)) tmp.Add(i);
            }
            uint id = tmp[new Random().Next(0, tmp.Count)];

            return new Tuple<uint, ShopNode>(id, new ShopNode(GetPrice(id)));
        }
    }
}
