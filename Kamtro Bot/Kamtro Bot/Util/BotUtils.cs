using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot
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

        /// <summary>
        /// This is a helper method to save an object to JSON given the location and object.
        /// </summary>
        /// <param name="location">The path of the file it will be saved to.</param>
        /// <param name="toWrite">The object to be saved.</param>
        public static void WriteToJson(object toWrite, string location) {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(location)) {
                using (JsonWriter writer = new JsonTextWriter(sw)) {
                    writer.Formatting = Formatting.Indented; // make it so that the entire file isn't on one line
                    serializer.Serialize(writer, toWrite);  // serialize the settings object and save it to the file
                }
            }
        }
    }
}
