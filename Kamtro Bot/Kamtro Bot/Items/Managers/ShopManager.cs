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
        public static Dictionary<int, ShopNode> Shop = new Dictionary<int, ShopNode>();

        public static int GetPrice(int itemid) {
            // TODO
            return Shop[itemid].Price;
        }
    }
}
