using Discord.WebSocket;
using Kamtro_Bot.Items;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Items
{
    public class ShopManager
    {
        public static List<ShopNode> Shop = new List<ShopNode>();

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
            for (int i = 0; i < 5; i++) {
                n = r.Next(0, options.Count);
                final.Add(options[n]);
                options.RemoveAt(n);
                if (options.Count == 0) break;
            }

            foreach(uint i in final) {
                Shop.Add(new ShopNode(i, ItemManager.GetItem(i).BuyPrice, ItemManager.GetItem(i).Buyable));
            }

            KLog.Info("Shop Refreshed.");

            return final;
        }

        public static List<uint> ValidateShopSelection() {
            List<ShopNode> toReplace = new List<ShopNode>();
            List<uint> replaced = new List<uint>();

            foreach(ShopNode i in Shop) {
                if(!ItemManager.GetItem(i.ItemID).Buyable) {
                    toReplace.Add(i);
                }
            }

            foreach(ShopNode i in toReplace) {
                Shop.Remove(i);

                ShopNode t = GetRandomNewSellableItem();

                Shop.Add(t);
                replaced.Add(t.ItemID);
            }

            return replaced;
        }

        /// <summary>
        /// This method adds an item to the shop.
        /// Precondition: The shop has been cleared.
        /// </summary>
        /// <param name="id">The ID of the item to add</param>
        /// <param name="price">The price of the item</param>
        /// <param name="avail">If the item is available</param>
        public static void AddItem(uint id, int price, bool avail) {
            ShopNode sn = new ShopNode(id, price, avail);
            Shop.Add(sn);
        }

        public static void AddItem(uint id, ItemInfoNode i) {
            AddItem(id, i.BuyPrice, i.Buyable);
        }

        public static int GetPrice(uint itemid) {
            return ItemManager.GetItem(itemid).BuyPrice;
        }

        public static bool GetAvailability(uint itemid) {
            return ItemManager.GetItem(itemid).Buyable;
        }

        public static void SetAvailability(uint id, bool a) {
            KLog.Info($"{(a ? "Enabled":"Disabled")} the item at id {id} in the shop");

            ItemManager.GetItem(id).Buyable = a;
            ValidateShopSelection();
        }

        public static bool BuyItem(ulong userid, int shopSlot, int quantity) {
            if (shopSlot > Shop.Count || shopSlot < 0) return false;

            SocketGuildUser user = BotUtils.GetGUser(userid);
            UserDataNode customer = UserDataManager.GetUserData(user);
            ShopNode item = Shop[shopSlot];

            if (quantity > 0 && customer.Money >= item.Price*quantity) {
                customer.Money -= item.Price * quantity;

                UserInventoryManager.GetInventory(userid).AddItem(item.ItemID, quantity);

                UserDataManager.SaveUserData();
                UserInventoryManager.SaveInventories();
                return true;
            }

            return false;
        }

        public static bool SellItem(ulong userid, uint itemid, int quantity) {
            UserInventoryNode i = UserInventoryManager.GetInventory(userid);

            if (i.ItemCount(itemid) < quantity) return false;

            int total = ItemManager.GetItem(itemid).GetSellPrice() * quantity;
            UserDataManager.GetUserData(BotUtils.GetGUser(userid)).Money += total;
            i.LoseItem(itemid, quantity);
            UserDataManager.SaveUserData();
            UserInventoryManager.SaveInventories();
            return true;
        }

        private static List<uint> GetSellableItems() {
            List<uint> options = new List<uint>();

            foreach (uint k in ItemManager.Items.Keys) {
                if (ItemManager.Items[k].Buyable) options.Add(k);
            }

            return options;
        }

        private static ShopNode GetRandomNewSellableItem() {
            List<uint> tmp = new List<uint>();
            bool add;

            foreach(uint i in GetSellableItems()) {
                add = true;

                foreach(ShopNode s in Shop) {
                    if (s.ItemID == i) add = false;
                }

                if (add) tmp.Add(i);
            }

            uint id = tmp[new Random().Next(0, tmp.Count)];

            return new ShopNode(id, GetPrice(id), true);
        }
    }
}
