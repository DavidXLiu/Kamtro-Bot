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

        public TimeZoneNode(string offset) {
            string[] match = offset.Split(':');
            Hour = int.Parse(match[0]);
            Minute = int.Parse(match[1]) * (Hour < 0 ? -1:1);
        }

        public TimeZoneNode(int h, int m) {
            Hour = h;
            Minute = m;
        }
    }
}
