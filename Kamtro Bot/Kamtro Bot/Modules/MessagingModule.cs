using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Util;
using Kamtro_Bot.Handlers;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// Module that is made for sending or receiving messages through the bot.
    /// </summary>
    public class MessagingModule : ModuleBase<SocketCommandContext>
    {
        [Command("directmessage")]
        [Alias("dm","pm","privatemessage","tell")]
        [Name("DirectMessage")]
        [Summary("Sends a direct message to the specified user.")]
        public async Task DirectMessageAsync([Remainder]string args = null)
        {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;

            SocketGuildUser user = BotUtils.GetGUser(Context);

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

        [Command("messagechannel")]
        [Alias("say", "speak")]
        [Name("MessageChannel")]
        [Summary("Sends a message to the specified channel.")]
        public async Task MessageChannelAsync([Remainder]string args = "")
        {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;

            if(string.IsNullOrWhiteSpace(args)) {
                await ReplyAsync(BotUtils.KamtroText("You need to say a message!"));
                return;
            }

            SocketTextChannel target = null;

            // Check if a channel name was specified
            string[] arrayCheck = Context.Message.Content.Split(' ');
            string channelToCheck;

            // Check if a channel was even in the message
            if (arrayCheck.Length > 1)
            {
                channelToCheck = arrayCheck[1];

                // Check if channel is mentioned in DMs
                if (Context.Channel is IDMChannel && channelToCheck.StartsWith("<#"))
                {
                    target = ServerData.Server.GetTextChannel(ulong.Parse(channelToCheck.Substring(2, channelToCheck.Length - 3)));
                }
                else
                {
                    // Check for name matches
                    foreach (SocketTextChannel textChannel in ServerData.Server.TextChannels)
                    {
                        if (UtilStringComparison.CompareWordScore(channelToCheck, textChannel.Name) > 0.66)
                        {
                            target = textChannel;
                            break;
                        }
                    }
                }
            }

            if (Context.Message.MentionedChannels.Count < 1) {
                await ReplyAsync(BotUtils.KamtroText("You need to specify the channel!"));
                return;
            }

            if (target == null)
                target = Context.Message.MentionedChannels.ElementAt(0) as SocketTextChannel;

            if(target == null) {
                await ReplyAsync(BotUtils.KamtroText("You need to specify a text channel!"));
                return;
            }

            List<string> msgl = args.Split(' ').ToList();

            msgl.RemoveAt(0);

            if(msgl.Count <= 0) {
                await ReplyAsync(BotUtils.KamtroText("You need to say a message!"));
                return;
            }

            string msg = string.Join(" ", msgl.ToArray());

            await target.SendMessageAsync(BotUtils.KamtroText(msg));
            await Context.Message.AddReactionAsync(ReactionHandler.CHECK_EM);

            KLog.Info($"Kamtro Message Sent in #{target.Name} from [{BotUtils.GetFullUsername(Context.User)}]: {msg}");
        }

        [Command("messagerelay")]
        [Alias("togglemessagerelay", "relaymessage", "relaymessages", "relay")]
        [Name("MessageRelay")]
        [Summary("Sends a message to the specified channel.")]
        /// Arcy
        public async Task MessageRelayAsync([Remainder]string message = "")
        {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;

            SocketGuildUser user = BotUtils.GetGUser(Context);

            // Check if admin
            if (user.GuildPermissions.Administrator || ServerData.AdminUsers.Contains(user))
            {
                // Check if in relay users collection
                if (ServerData.RelayUsers.Contains(user))
                {
                    await ReplyAsync(BotUtils.KamtroText("You will no longer receive direct messages sent to Kamtro bot."));
                    ServerData.RelayUsers.Remove(user);
                }
                else
                {
                    await ReplyAsync(BotUtils.KamtroText("You will receive direct messages sent to Kamtro bot."));
                    ServerData.RelayUsers.Add(user);
                }
            }
        }
    }
}
