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
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot
{
    /// <summary>
    /// <para>This is the class that contains the bot's settings.</para>
    /// <para>Loaded from the settings.json file.</para>
    /// </summary>
    public class BotSettings
    {
        public string Prefix;
        /// <summary>
        /// Discord ID for the Kamtro Server.
        /// </summary>
        public ulong KamtroID;  // The server ID
        public ulong BotChannelID;  // The ID of the bot channel

        // Roles
        public List<ulong> ModifiableRoles;
        public List<ulong> ModeratorRoles;
        public List<ulong> TrustedRoles;
        public List<ulong> SilencedRoles;

        public Dictionary<ulong, RoleInfoNode> RoleDescriptions;

        public ulong KamtroBotRoleId;
        public ulong NSFWRole;

        // Users
        public List<ulong> AdminUsers;

        public List<ulong> RelayUsers;


        public ulong PrimaryContactUserId; // Should always be Arcy, but made it just in case it needs to be changed - Arcy

        /// <summary>
        /// Constructor for the BotSettings Class. 
        /// </summary>
        /// -C
        /// <param name="prefix">The prefix for bot commands.</param>
        /// <param name="kamtro_id">The ID of the Kamtro Server.</param>
        /// <param name="bot_channel_id">The ID of the bot channel</param>
        /// <param name="roleInfo">Custom role descriptions for the menus</param>
        public BotSettings(string prefix, ulong kamtro_id = 390720675486367744, ulong bot_channel_id = 390982003455164426, Dictionary<ulong, RoleInfoNode> roleInfo = null) {
            Prefix = prefix;
            KamtroID = kamtro_id;
            BotChannelID = bot_channel_id;

            if (roleInfo == null) {
                RoleDescriptions = new Dictionary<ulong, RoleInfoNode>();

                RoleDescriptions.Add(1234, new RoleInfoNode("exampleRole", "Change Me", "0xFFF000"));
                RoleDescriptions.Add(1221234, new RoleInfoNode("Other ROle ", "Change Me as well please", "0xCAFE87"));
                RoleDescriptions.Add(71294710938741928, new RoleInfoNode("MemberOrSomethin", "Change Me the 3rd", "0xAFEx897"));
            } else {
                RoleDescriptions = roleInfo;
            }

            ModifiableRoles = new List<ulong>();
            ModeratorRoles = new List<ulong>();
            TrustedRoles = new List<ulong>();
            SilencedRoles = new List<ulong>();

            RelayUsers = new List<ulong>();
            AdminUsers = new List<ulong>();
        }

        /// <summary>
        /// This is a static method that loads the settings.json file and 
        /// </summary>
        /// <returns></returns>
        public static BotSettings LoadJson() {
            string fileTxt = FileManager.ReadFullFile(DataFileNames.GeneralConfigFile); // read the config file
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
