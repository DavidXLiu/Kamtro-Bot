using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

using Kamtro_Bot.Util;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// Module that is made for sending or receiving messages through the bot.
    /// </summary>
    class MessagingModule : ModuleBase<SocketCommandContext>
    {
        [Command("directmessage"), Alias("dm","pm","privatemessage","tell")]
        [Name("DirectMessage")]
        [Summary("Sends a direct message to the specified user.")]
        public async Task DirectMessageAsync([Remainder]string message)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);

            // Check if admin
            if (user.GuildPermissions.Administrator || ServerData.AdminUsers.Contains(user))
            {
                /// TO DO
                /// 
                /// Get all users in remainder of the message
                /// Make prompt on what message to send to the users
                /// Include attachments
                /// Prompt should have a cancel and confirm button
                /// Sends the message to the users when confirmed
            }
        }

        [Command("messagechannel"), Alias("say", "speak")]
        [Name("MessageChannel")]
        [Summary("Sends a message to the specified channel.")]
        public async Task MessageChannelAsync([Remainder]string message)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);

            // Check if admin
            if (user.GuildPermissions.Administrator || ServerData.AdminUsers.Contains(user))
            {
                /// TO DO
                /// 
                /// Make toggleable option to record messages received in DMs and send them to specified channel
                /// Single messages should work similar to the DM command
            }
        }

        [Command("messagerelay"), Alias("togglemessagerelay", "relaymessage", "relaymessages", "relay")]
        [Name("MessageRelay")]
        [Summary("Sends a message to the specified channel.")]
        /// Arcy
        public async Task MessageRelayAsync([Remainder]string message)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);

            // Check if admin
            if (user.GuildPermissions.Administrator || ServerData.AdminUsers.Contains(user))
            {
                // Check if in relay users collection
                if (ServerData.RelayUsers.Contains(user))
                {
                    ServerData.RelayUsers.Remove(user);
                }
                else
                {
                    ServerData.RelayUsers.Add(user);
                }
            }
        }
    }
}
