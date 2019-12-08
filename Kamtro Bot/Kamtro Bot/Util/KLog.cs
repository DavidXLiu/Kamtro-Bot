using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot
{
    public static class KLog {
        public static int LOG_MAX = 40;

        public static List<string> log = new List<string>();

        public static void Debug(string msg) {
            if(Program.Debug) {
                Console.WriteLine("[debug] " + msg);
                PushList("[debug] " + msg);
            }
        }

        public static void Info(string msg) {
            Console.WriteLine("[i] " + msg);
            PushList("[i] " + msg);
        }

        public static void Important(string msg) {
            Console.WriteLine("[I] " + msg);
            PushList("[I] " + msg);
        }

        public static void Warning(string msg) {
            Console.WriteLine("[W] " + msg);
            PushList("[W] " + msg);
        }

        public static void Error(string msg) {
            Console.WriteLine("[ERROR] " + msg);
            PushList("[ERROR] " + msg);
        }

        private static void PushList(string msg) {
            log.Add(msg);

            if (log.Count > LOG_MAX) log.RemoveAt(0);
        }
    }
}
