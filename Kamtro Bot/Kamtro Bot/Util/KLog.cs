using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot
{
    public static class KLog {
        public static void Debug(string msg) {
            if(Program.Debug) {
                Console.WriteLine("[debug] " + msg);
            }
        }

        public static void Info(string msg) {
            Console.WriteLine("[i] " + msg);
        }

        public static void Important(string msg) {
            Console.WriteLine("[I] " + msg);
        }

        public static void Warning(string msg) {
            Console.WriteLine("[W] " + msg);
        }

        public static void Error(string msg) {
            Console.WriteLine("[ERROR] " + msg);
        }
    }
}
