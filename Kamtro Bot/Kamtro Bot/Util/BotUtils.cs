using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Util
{
    /// <summary>
    /// General utility functions for Kamtro
    /// -C
    /// </summary>
    public class BotUtils
    {
        /// <summary>
        /// Formats the message so the text is blue (AKA Kamtro speak)
        /// -C
        /// </summary>
        /// <param name="message">The message you want to put into kamtro-formatting</param>
        /// <returns>The message in kamtro-formatting</returns>
        public static string KamtroText(string message) {
            return $"```INI\n{message}\n```";
        }
    }
}
