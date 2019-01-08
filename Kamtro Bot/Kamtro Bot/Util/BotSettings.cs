using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Discord.Commands;
using Kamtro_Bot.Managers;
using System.IO;

namespace Kamtro_Bot
{
    /// <summary>
    /// <para>This is the class that contains the bot's settings.</para>
    /// <para>Loaded from the settings.json file.</para>
    /// </summary>
    public class BotSettings
    {
        public string Prefix;

        public BotSettings(string prefix) {
            Prefix = prefix;
        }

        /// <summary>
        /// This is a static method that loads the settings.json file and 
        /// </summary>
        /// <returns></returns>
        public static BotSettings LoadJson() {
            string fileTxt = new FileManager().ReadFullFile(DataFileNames.CommandSettingsFile); // read the config file
            BotSettings loaded = JsonConvert.DeserializeObject<BotSettings>(fileTxt);  // deserialize the json into an object

            return loaded;
        }

        /// <summary>
        /// This method saves the current BotSettings object to the JSON file.
        /// Not static because it is to be called on the newest instance.
        /// </summary>
        public void SaveJson() {
            JsonSerializer serializer = new JsonSerializer();
            using(StreamWriter sw = new StreamWriter(DataFileNames.CommandSettingsFile)) {
                using (JsonWriter writer = new JsonTextWriter(sw)) {
                    writer.Formatting = Formatting.Indented; // make it so that the entire file isn't on one line
                    serializer.Serialize(writer, this);  // serialize the settings object and save it to the file
                }
            }
        }
    }
}
