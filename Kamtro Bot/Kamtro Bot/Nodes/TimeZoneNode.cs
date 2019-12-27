using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    public class TimeZoneNode
    {
        public int Hour;
        public int Minute;

        public TimeZoneNode(int h, int m) {
            Hour = h;
            Minute = m;
        }
    }
}
