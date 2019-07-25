using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot
{
    public class KLog {
        public static void Info(string msg) {
            Console.WriteLine("[i] " + msg);
        }

        public static void Important(string msg) {
            Console.WriteLine("[I] " + msg);
        }

        public static void Warning(string msg) {
            Console.WriteLine("[W] " + msg);
        }
    }
}
