using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Kamtro_Bot.Util
{
    /// <summary>
    /// Holds all data that is obtained from the server. - Arcy
    /// </summary>
    public class ServerData
    {
        public List<SocketRole> AllRoles;
        public List<SocketRole> ModifiableRoles;
        public List<SocketRole> ModeratorRoles;
        public SocketRole NSFWRole;

        private ulong ServerId;

        /// <summary>
        /// Call this method when the client connects to the guild.
        /// Gets all necessary data from the server and populates data collections.
        /// </summary>
        /// Arcy
        /// <returns>Returns whether the method was successful or not.</returns>
        public bool InitializeServerData(DiscordSocketClient client)
        {
            // Initialize collections
            ModifiableRoles = new List<SocketRole>();
            ModeratorRoles = new List<SocketRole>();

            // Read in file data
            /// TO DO

            // All Roles
            AllRoles = client.GetGuild(ServerId).Roles.ToList();

            // Temporary
            return true;
        }
    }
}
