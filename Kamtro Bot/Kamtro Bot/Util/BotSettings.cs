using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Discord.Commands;
using Kamtro_Bot.Managers;
using System.IO;
using Discord.WebSocket;

namespace Kamtro_Bot
{
    /// <summary>
    /// <para>This is the class that contains the bot's settings.</para>
    /// <para>Loaded from the settings.json file.</para>
    /// </summary>
    public class BotSettings
    {
        public static List<SocketRole>

        public string Prefix;
        public ulong KamtroID;  // The server ID
        public ulong BotChannelID;  // The ID of the bot channel

        // Roles
        public List<ulong> ModifiableRoles;
        public List<ulong> ModeratorRoles;

        public ulong NSFWRole;

        /// <summary>
        /// Constructor for the BotSettings Class. 
        /// </summary>
        /// -C
        /// <param name="prefix">The prefix for bot commands.</param>
        /// <param name="kamtro_id">The ID of the Kamtro Server.</param>
        /// <param name="bot_channel_id">The ID of the bot channel</param>
        public BotSettings(string prefix, ulong kamtro_id = 390720675486367744, ulong bot_channel_id = 390982003455164426) {
            Prefix = prefix;
            KamtroID = kamtro_id;
            BotChannelID = bot_channel_id;
        }

        /// <summary>
        /// This is a static method that loads the settings.json file and 
        /// </summary>
        /// <returns></returns>
        public static BotSettings LoadJson() {
            string fileTxt = new FileManager().ReadFullFile(DataFileNames.GeneralConfigFile); // read the config file
            BotSettings loaded = JsonConvert.DeserializeObject<BotSettings>(fileTxt);  // deserialize the json into an object

            return loaded;
        }

        /// <summary>
        /// This method saves the current BotSettings object to the JSON file.
        /// Not static because it is to be called on the newest instance.
        /// </summary>
        public void SaveJson() {
            BotUtils.WriteToJson(this, DataFileNames.GeneralConfigFile);
        }
    }
}
