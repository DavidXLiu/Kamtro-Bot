﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// Module that is made for sending or receiving messages through the bot.
    /// </summary>
    public class MessagingModule : ModuleBase<SocketCommandContext>
    {
        [Command("directmessage"), Alias("dm","pm","privatemessage","tell")]
        [Name("DirectMessage")]
        [Summary("Sends a direct message to the specified user.")]
        public async Task DirectMessageAsync([Remainder]string args = null)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);

            // Check if admin
            if (user.GuildPermissions.Administrator || ServerData.AdminUsers.Contains(user))
            {
                /// TO DO
                /// 
                /// Get the user in remainder of the message [DONE]
                /// Make prompt on what message to send to the user [DONE]
                /// Include attachments
                /// Sends the message to the user [DONE]
                
                // Inform user to specify the user to DM
                if (args == null)
                {
                    await ReplyAsync(BotUtils.KamtroText("You must specify the user to directly message."));
                    return;
                }
                else
                {
                    // Split the message to get the corresponding parts
                    string[] msgSplit = Context.Message.Content.Split(' ');
                    SocketGuildUser target = BotUtils.GetUser(msgSplit[1]);

                    if (target == null)
                    {
                        await ReplyAsync(BotUtils.KamtroText("I cannot find that user."));
                        return;
                    }
                    else if (msgSplit.Length == 2)
                    {
                        /// To Do:
                        /// Show embed for messaging the user [DONE]
                        /// Toggle on messaging that user
                        
                        MessagingEmbed embed = new MessagingEmbed(user);
                        // Send embed in DMs to user
                        try
                        {
                            await embed.Display((IMessageChannel)user.GetOrCreateDMChannelAsync());
                        }
                        // Could not send to the user
                        catch (Exception)
                        {
                            if (user.Nickname != null)
                            {
                                await ReplyAsync(BotUtils.KamtroText($"I cannot send direct messages to you, {user.Nickname}. Please allow direct messages from me."));
                            }
                            else
                            {
                                await ReplyAsync(BotUtils.KamtroText($"I cannot send direct messages to you, {user.Username}. Please allow direct messages from me."));
                            }
                        }
                    }
                    else
                    {
                        // Build the message back together
                        string msgSend = "";
                        for (int i = 2; i < msgSplit.Length; i++)
                        {
                            msgSend += msgSplit[i];
                            if (i != msgSplit.Length - 1)
                            {
                                msgSend += " ";
                            }
                        }

                        // Send message and notify user using the command
                        try
                        {
                            await target.SendMessageAsync(msgSend);
                            await ReplyAsync(BotUtils.KamtroText($"Message sent to {target.Username}#{user.Discriminator}."));
                        }
                        // Could not send to the user
                        catch(Exception)
                        {
                            await ReplyAsync(BotUtils.KamtroText($"Message could not be sent to the user. The user either has messages from only friends allowed or has blocked me."));
                        }
                    }
                }
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
