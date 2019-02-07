using System;
using System.Collections.Generic;
using Discord.WebSocket;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Managers
{
    /// <summary>
    /// Security manager. This handles validation of messages, and spam handling.
    /// </summary>
    /// -C
    public class SecurityManager
    {
        private static Dictionary<ulong, SecurityMessageNode> MessageCount = new Dictionary<ulong, SecurityMessageNode>();

        /// <summary>
        /// Checks the message.
        /// </summary>
        /// <remarks>
        /// This method is used to perform any checks on the message for security.
        /// This is also used to handle spam etc.
        /// This is called whenever there is a valid message.
        /// Message will never be null or invalid.
        /// 
        /// -C
        /// </remarks>
        /// <param name="message">The message to be checked</param>
        public static void CheckMessage(SocketUserMessage message) {
            if (MessageExempt(message)) return;
        }

        /// <summary>
        /// Tests to see if the message shouldn't be checked for spam
        /// </summary>
        /// <returns><c>true</c>, if message is from a trusted user, <c>false</c> otherwise.</returns>
        /// <param name="msg">Message.</param>
        public static bool MessageExempt(SocketUserMessage msg) {
            SocketGuildUser author = msg.Author as SocketGuildUser;

            // O(1) Checks
            if (author.Guild.Owner.Id == author.Id) return true;
            if (author.GuildPermissions.ManageMessages) return true;
            if (author.GuildPermissions.AddReactions) return true; 

            // O(n) Checks
            if (ServerData.AdminUsers.Contains(author)) return true;


            // O(nk) checks
            foreach(SocketRole trustedRole in ServerData.TrustedRoles) {
                foreach(SocketRole role in author.Roles) {
                    if (trustedRole.Id == role.Id) return true;
                }
            }

            return false;
        }
    }
}
