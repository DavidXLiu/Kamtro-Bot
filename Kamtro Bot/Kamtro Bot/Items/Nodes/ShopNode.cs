using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Items
{
    public class ShopNode
    {
        public int Price;
        public bool Available;

        public ShopNode(int price, bool available = false) {
            Price = price;
            Available = available;
        }
    }
}
