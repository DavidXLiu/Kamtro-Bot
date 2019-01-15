using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.WebSocket;

namespace Kamtro_Bot.Util
{
    /// <summary>
    /// A class used to convert data saved in Json files into Discord's data classes.
    /// Initialize this class after all the data is read in from the Json files.
    /// </summary>
    class ServerData
    {
        public static List<SocketRole> AllRoles;
        public static List<SocketRole> ModifiableRoles;
        public static List<SocketRole> ModeratorRoles;

        /// <summary>
        /// Goes through the process of initializing and populating all data collections with the passed in data.
        /// </summary>
        /// <param name="bs"></param>
        public ServerData(BotSettings bs)
        {
            DiscordSocketClient client = Program.Instance.Client;
            SocketGuild server = client.GetGuild(bs.KamtroID);

            #region AllRoles
            // Add all server roles to the AllRoles collection. - Arcy
            AllRoles = server.Roles.ToList();
            #endregion

            #region ModifiableRoles & ModeratorRoles
            // Loop through each role id and add the SocketRole to the collection it is in. - Arcy
            foreach (SocketRole role in server.Roles)
            {
                // Modifiable Roles
                foreach (ulong roleId in bs.ModifiableRoles)
                {
                    // When finding a match, add to the collection. - Arcy
                    if (role.Id == roleId)
                    {
                        ModifiableRoles.Add(role);
                        break;
                    }
                }

                // Moderator Roles
                foreach (ulong roleId in bs.ModeratorRoles)
                {
                    // When finding a match, add to the collection. - Arcy
                    if (role.Id == roleId)
                    {
                        ModeratorRoles.Add(role);
                        break;
                    }
                }
            }
            #endregion
        }
    }
}
