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
        // Server
        public static SocketGuild Server;

        // Roles
        public static List<SocketRole> AllRoles;
        public static List<SocketRole> ModifiableRoles;
        public static List<SocketRole> ModeratorRoles;
        public static List<SocketRole> TrustedRoles;

        public static Dictionary<SocketRole, RoleInfoNode> RoleInfo;

        public static SocketRole KamtroBotRole;
        public static SocketRole NSFWRole;

        // Users
        public static List<SocketGuildUser> AdminUsers;

        public static List<SocketGuildUser> RelayUsers;

        public static SocketGuildUser PrimaryContactUser;

        /// <summary>
        /// Permission level enum. For checking perms easily
        /// </summary>
        public enum PermissionLevel
        {
            MUTED,
            USER,
            TRUSTED,
            MODERATOR,
            ADMIN
        }

        /// <summary>
        /// Goes through the process of initializing and populating all data collections with the passed in data.
        /// </summary>
        /// <param name="bs"></param>
        public static void SetupServerData(BotSettings bs) {
            DiscordSocketClient client = Program.Client;
            Server = client.GetGuild(bs.KamtroID);

            #region AllRoles
            // Add all server roles to the AllRoles collection. - Arcy
            AllRoles = Server.Roles.ToList();
            #endregion

            #region ModifiableRoles, TrustedRoles & ModeratorRoles
            ModifiableRoles = new List<SocketRole>();
            ModeratorRoles = new List<SocketRole>();
            TrustedRoles = new List<SocketRole>();

            // Loop through each role id and add the SocketRole to the collection it is in. - Arcy
            foreach (SocketRole role in Server.Roles) {
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

                // Trusted roles (Member, Veteran)
                foreach (ulong roleId in bs.TrustedRoles) {
                    if(role.Id == roleId) {
                        TrustedRoles.Add(role);
                    }
                }
            }
            #endregion

            #region Individual Roles
            KamtroBotRole = Server.GetRole(bs.KamtroBotRoleId);
            NSFWRole = Server.GetRole(bs.NSFWRole);
            #endregion

            #region Admin Users
            AdminUsers = new List<SocketGuildUser>();
            // Read in all recorded ids
            foreach (ulong userId in bs.AdminUsers)
            {
                SocketGuildUser user = Server.GetUser(userId);
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
                SocketGuildUser user = Server.GetUser(userId);
                RelayUsers.Add(user);
            }
            #endregion

            #region Individual Users
            PrimaryContactUser = Server.GetUser(bs.PrimaryContactUserId);
            #endregion
        }



        public static bool HasPermissionLevel(SocketGuildUser user, PermissionLevel level) {
            switch(level) {
                case PermissionLevel.USER:
                    return true;

                case PermissionLevel.TRUSTED:
                    if (IsTrusted(user)) return true;
                    goto case PermissionLevel.MODERATOR;

                case PermissionLevel.MODERATOR:
                    if (IsModerator(user)) return true;
                    goto case PermissionLevel.ADMIN;

                case PermissionLevel.ADMIN:
                    if (IsAdmin(user)) return true;
                    return false;

                default:
                    return false;
            }
        }

        public static bool IsAdmin(SocketGuildUser user) {
            foreach(SocketGuildUser adminUser in AdminUsers) {
                if(adminUser.Id == user.Id) {
                    return true;
                }
            }

            return false;

        }

        public static bool IsModerator(SocketGuildUser user) {
            foreach (SocketRole mod_role in ModeratorRoles) {
                foreach(SocketRole role in user.Roles) {
                    if(mod_role.Id == role.Id) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsTrusted(SocketGuildUser user) {
            foreach (SocketRole trustedRole in ServerData.TrustedRoles) {
                foreach (SocketRole role in user.Roles) {
                    if (trustedRole.Id == role.Id) return true;
                }
            }

            return false;
        }
    }
}
