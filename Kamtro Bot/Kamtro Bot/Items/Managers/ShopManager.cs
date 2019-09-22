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
        }
    }
}
