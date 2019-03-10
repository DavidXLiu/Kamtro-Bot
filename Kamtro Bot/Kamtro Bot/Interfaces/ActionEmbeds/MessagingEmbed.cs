using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces
{
    public class MessagingEmbed : ActionEmbed
    {
        public const string MODIFY = "\u1f537";
        public const string END = "\u1f51a";

        private SocketGuildChannel targetChannel;
        private SocketGuildUser targetUser;
        private SocketUserMessage lastMessage = null;
        private bool kamtroText = false;

        public MessagingEmbed(SocketGuildUser sender, SocketGuildChannel channel = null, SocketGuildUser user = null)
        {
            AddMenuOptions(new MenuOptionNode(MODIFY, "Toggle Kamtro Text"),
                new MenuOptionNode(END, "End"));

            CommandCaller = sender;

            if (channel != null)
            {
                targetChannel = channel;
            }
            else if (user != null)
            {
                targetUser = user;
            }
        }

        public override Embed GetEmbed()
        {
            EmbedBuilder builder = new EmbedBuilder();

            if (targetChannel != null)
            {
                builder.WithAuthor($"Messaging {targetChannel.Name}", targetChannel.Guild.IconUrl);
                builder.WithDescription($"Your direct messages to me will be sent to <#{targetChannel.Id}>.");

                string lastMessageStr = "";
                if (lastMessage != null)
                {
                    lastMessageStr += lastMessage.Content;
                }
                else
                {
                    lastMessageStr += "**No messages sent yet.**";
                }

                if (kamtroText)
                {
                    lastMessageStr += "\n\nKamtro Text: **ON**";
                    builder.WithColor(30, 225, 235);
                }
                else
                {
                    lastMessageStr += "\n\nKamtro Text: **OFF**";
                    builder.WithColor(0, 0, 0);
                }

                builder.AddField("Last Message Sent", lastMessageStr);
            }
            else if (targetUser != null)
            {
                builder.WithAuthor($"Messaging {targetUser.Username}#{targetUser.Discriminator}", targetUser.GetAvatarUrl());
                builder.WithDescription($"Your direct messages to me will be sent to {targetUser.Mention}.");

                string lastMessageStr = "";
                if (lastMessage != null)
                {
                    lastMessageStr += lastMessage.Content;
                }
                else
                {
                    lastMessageStr += "**No messages sent yet.**";
                }

                if (kamtroText)
                {
                    lastMessageStr += "\n\nKamtro Text: **ON**";
                    builder.WithColor(30, 225, 235);
                }
                else
                {
                    lastMessageStr += "\n\nKamtro Text: **OFF**";
                    builder.WithColor(0, 0, 0);
                }

                builder.AddField("Last Message Sent", lastMessageStr);
            }

            AddMenu(builder);

            return builder.Build();
        }

        public async override Task PerformAction(SocketReaction option)
        {
            switch (option.Emote.ToString())
            {
                case MODIFY:
                    kamtroText = !kamtroText;
                    await Message.ModifyAsync(x => x.Embed = GetEmbed());
                    break;
                case END:
                    if (targetChannel != null)
                    {
                        await option.Channel.SendMessageAsync(BotUtils.KamtroText($"You will no longer send messages to {targetChannel.Name}."));
                    }
                    else if (targetUser != null)
                    {
                        await option.Channel.SendMessageAsync(BotUtils.KamtroText($"You will no longer send messages to {targetUser.Username}#{targetUser.Discriminator}."));
                    }
                    EventQueueManager.RemoveEvent(this);
                    break;
            }
        }
    }
}
