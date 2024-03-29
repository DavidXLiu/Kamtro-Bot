﻿using System;
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
        #region Variables
        // Main Server
        public static SocketGuild Server;

        // Other Servers
        public static SocketGuild Kamexico;
        public static SocketGuild Retropolis;

        // Channels
        public static SocketTextChannel AdminChannel;
        public static SocketTextChannel LogChannel;
        public static SocketTextChannel BotChannel;

        // Roles
        public static List<SocketRole> AllRoles;
        public static List<SocketRole> ModifiableRoles;
        public static List<SocketRole> ModeratorRoles;
        public static List<SocketRole> TrustedRoles;
        public static List<SocketRole> SilencedRoles;

        public static Dictionary<SocketRole, RoleInfoNode> RoleInfo;

        public static SocketRole KamtroBotRole;
        public static SocketRole NSFWRole;
        public static SocketRole Kamexican;
        public static SocketRole Retropolitan;
        public static SocketRole Lurker;

        // Users
        public static List<SocketGuildUser> AdminUsers;
        public static List<SocketGuildUser> RelayUsers;
        public static SocketGuildUser PrimaryContactUser;
        
        // Bug Fixing Data
        public static List<ulong> AdminUserIDs;
        public static List<ulong> ModRoleIDs;

        // Information
        public static SocketUser BannedUser; // Used to track what user to delete messages from for the DeleteBanMessages command
        #endregion
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
        #region Methods
        /// <summary>
        /// Goes through the process of initializing and populating all data collections with the passed in data.
        /// </summary>
        /// <param name="bs"></param>
        public static void SetupServerData(BotSettings bs) {
            DiscordSocketClient client = Program.Client;
            Server = client.GetGuild(bs.KamtroID);
        
            #region Other Servers
            if (bs.KamexicoID == 0) {
                Kamexico = null;
            } else {
                Kamexico = client.GetGuild(bs.KamexicoID);
            }

            if(bs.RetropolisID == 0) {
                Retropolis = null;
            } else {
                Retropolis = client.GetGuild(bs.RetropolisID);
            }
            #endregion

            #region Channels
            AdminChannel = Server.GetTextChannel(bs.AdminChannelID);
            LogChannel = Server.GetTextChannel(bs.LogChannelID);
            BotChannel = Server.GetTextChannel(bs.BotChannelID);
            #endregion

            #region AllRoles
            // Add all server roles to the AllRoles collection. - Arcy
            AllRoles = Server.Roles.ToList();
            #endregion

            #region ModifiableRoles, SilencedRoles, TrustedRoles & ModeratorRoles
            ModifiableRoles = new List<SocketRole>();
            ModeratorRoles = new List<SocketRole>();
            TrustedRoles = new List<SocketRole>();
            SilencedRoles = new List<SocketRole>();
            
            AdminUserIDs = new List<ulong>();
            ModRoleIDs = new List<ulong>();
            
            // Loop through each role id and add the SocketRole to the collection it is in. - Arcy
            foreach (SocketRole role in Server.Roles) {
                // Moderator Roles
                foreach (ulong roleId in bs.ModeratorRoles) {
                    // When finding a match, add to the collection. - Arcy
                    ModRoleIDs.add(roleId);
                    
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

                // Silenced roles
                foreach(ulong roleId in bs.SilencedRoles) {
                    if(role.Id == roleId) {
                        SilencedRoles.Add(role);
                    }
                }
            }

            // Order matters for modifiable roles - Arcy
            // Modifiable Roles
            foreach (ulong roleId in bs.ModifiableRoles)
            {
                foreach (SocketRole role in Server.Roles)
                {
                    // When finding a match, add to the collection. - Arcy
                    if (role.Id == roleId)
                    {
                        ModifiableRoles.Add(role);
                        break;
                    }
                }
            }
            #endregion

            #region Individual Roles
            KamtroBotRole = Server.GetRole(bs.KamtroBotRoleId);
            NSFWRole = Server.GetRole(bs.NSFWRole);
            Kamexican = Server.GetRole(bs.KamexicanID);
            Retropolitan = Server.GetRole(bs.RetropolitanID);
            Lurker = Server.GetRole(bs.LurkerID);
            #endregion

            #region Admin Users
            AdminUsers = new List<SocketGuildUser>();
            // Read in all recorded ids
            foreach (ulong userId in bs.AdminUsers)
            {
                AdminUserIDs.add(userId);
                
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
                case PermissionLevel.MUTED:
                    return IsSilenced(user);

                case PermissionLevel.USER:
                    return !IsSilenced(user);

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
            if(AdminUserIDs.Contains(user.Id) return true;
            
            foreach(SocketGuildUser adminUser in AdminUsers) {
                if(adminUser.Id == user.Id) {
                    return true;
                }
            }

            return false;
        }

        public static bool IsModerator(SocketGuildUser user) {
            foreach (ulong mod_role_id in ModRoleIDs) {
                foreach(SocketRole role in user.Roles) {
                    if(mod_role_id.Id == role.Id) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsTrusted(SocketGuildUser user) {
            foreach (SocketRole trustedRole in TrustedRoles) {
                foreach (SocketRole role in user.Roles) {
                    if (trustedRole.Id == role.Id) return true;
                }
            }

            return false;
        }

        public static bool IsSilenced(SocketGuildUser user) {
            foreach (SocketRole silencedRole in SilencedRoles) {
                foreach (SocketRole role in user.Roles) {
                    if (silencedRole.Id == role.Id) return true;
                }
            }

            return false;
        }
        #endregion
    }
}
