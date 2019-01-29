using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.WebSocket;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Util
{
    /// <summary>
    /// A class used to convert data saved in Json files into Discord's data classes.
    /// Initialize this class after all the data is read in from the Json files.
    /// </summary>
    public class ServerData
    {
        // Roles
        public static List<SocketRole> AllRoles;
        public static List<SocketRole> ModifiableRoles;
        public static List<SocketRole> ModeratorRoles;

        public static Dictionary<SocketRole, RoleInfoNode> RoleInfo;

        public static SocketRole KamtroBotRole;
        public static SocketRole NSFWRole;

        // Users
        public static List<SocketGuildUser> AdminUsers;

        public static List<SocketGuildUser> RelayUsers;

        public static SocketGuildUser PrimaryContactUser;

        /// <summary>
        /// Goes through the process of initializing and populating all data collections with the passed in data.
        /// </summary>
        /// <param name="bs"></param>
        public static void SetupServerData(BotSettings bs) {
            DiscordSocketClient client = Program.Client;
            SocketGuild server = client.GetGuild(bs.KamtroID);

            #region AllRoles
            // Add all server roles to the AllRoles collection. - Arcy
            AllRoles = server.Roles.ToList();
            #endregion
            #region ModifiableRoles & ModeratorRoles
            ModifiableRoles = new List<SocketRole>();
            ModeratorRoles = new List<SocketRole>();

            // Loop through each role id and add the SocketRole to the collection it is in. - Arcy
            foreach (SocketRole role in server.Roles) {
                // Modifiable Roles
                foreach (ulong roleId in bs.ModifiableRoles) {
                    // When finding a match, add to the collection. - Arcy
                    if (role.Id == roleId) {
                        ModifiableRoles.Add(role);
                        break;
                    }
                }

                // Moderator Roles
                foreach (ulong roleId in bs.ModeratorRoles) {
                    // When finding a match, add to the collection. - Arcy
                    if (role.Id == roleId) {
                        ModeratorRoles.Add(role);
                        break;
                    }
                }
            }
            #endregion

            #region Individual Roles
            KamtroBotRole = server.GetRole(bs.KamtroBotRoleId);
            NSFWRole = server.GetRole(bs.NSFWRole);
            #endregion

            #region Admin Users
            AdminUsers = new List<SocketGuildUser>();
            // Read in all recorded ids
            foreach (ulong userId in bs.AdminUsers)
            {
                SocketGuildUser user = server.GetUser(userId);
                // Check if user is in server
                if (user != null)
                {
                    // Add to collection if in server
                    AdminUsers.Add(user);
                }
            }
            #endregion

            #region Relay Users
            RelayUsers = new List<SocketGuildUser>();
            // Read in all recorded ids
            foreach (ulong userId in bs.RelayUsers)
            {
                SocketGuildUser user = server.GetUser(userId);
                RelayUsers.Add(user);
            }
            #endregion

            #region Individual Users
            PrimaryContactUser = server.GetUser(bs.PrimaryContactUserId);
            #endregion
        }
    }
}
