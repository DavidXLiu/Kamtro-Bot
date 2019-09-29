using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Items
{
    public class ShopNode
    {
        public uint ItemID;
        public int Price;
        public bool Available;

        public ShopNode(uint id, int price, bool available = false) {
            ItemID = id;
            Price = price;
            Available = available;
        }
    }
}
