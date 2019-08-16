using System;
using System.Collections.Generic;
using System.Linq; 
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class MessageDeleteEmbed : KamtroEmbedBase
    {
        private SocketUser Author;
        private string AuthorName;
        private string AuthorMention;
        private string Channel;
        private string Text;
        private DateTimeOffset MessageTimestamp;
        private bool TooOld;

        public MessageDeleteEmbed(SocketMessage msg, string sendTime = "", SocketTextChannel channel = null) {
            if (msg == null) {
                Text = "Either I forgot the message, or it was sent before I woke up!";
                Author = null;
                AuthorName = "Unknown User";
                AuthorMention = null;
                MessageTimestamp = DateTimeOffset.Now;
                TooOld = true;
                Channel = channel == null ? "Unknown Channel" : channel.Mention;
            } else {
                Text = msg.Content;
                Author = msg.Author;
                AuthorName = Author.Username + "#" + Author.Discriminator;
                AuthorMention = Author.Mention;
                MessageTimestamp = msg.Timestamp;
                Channel = channel == null ? (ServerData.Server.GetChannel(msg.Channel.Id) as SocketTextChannel).Mention : channel.Mention;
                TooOld = false;
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle(AuthorName);
            eb.WithDescription($"Message from {AuthorMention} deleted in {Channel}");
            eb.WithColor(BotUtils.Orange);

            if(!TooOld) { 
                eb.WithThumbnailUrl(Author.GetAvatarUrl());
                eb.AddField("Message Content", Text);
            } else {
                eb.AddField("Message Content Unavailable", Text);
            }

            eb.Timestamp = MessageTimestamp;

            return eb.Build();
        }
    }
}
