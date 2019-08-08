using System;
using System.Collections.Generic;
using System.Linq; 
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class MessageDeleteEmbed : KamtroEmbedBase
    {
        private SocketUser Author;
        private string AuthorName;
        private string Channel;
        private string Text;
        private string MessageTimestamp;
        private bool TooOld;

        public MessageDeleteEmbed(SocketMessage msg, string sendTime = "", SocketTextChannel channel = null) {
            if (msg == null) {
                Text = "Either I forgot the message, or it was sent before I woke up!";
                Author = null;
                AuthorName = "Unknown User";
                MessageTimestamp = "Unknown Time";
                TooOld = true;
                Channel = channel == null ? "Unknown Channel" : channel.Name;
            } else {
                Text = msg.Content;
                Author = msg.Author;
                AuthorName = BotUtils.GetFullUsername(msg.Author);
                MessageTimestamp = msg.Timestamp.LocalDateTime.ToLongDateString();
                Channel = channel == null ? msg.Channel.Name : channel.Name;
                TooOld = false;
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle($"Message from {AuthorName} deleted in #{Channel}");
            eb.WithColor(BotUtils.Orange);

            if(!TooOld) { 
                eb.WithThumbnailUrl(Author.GetAvatarUrl());
                eb.AddField("Message Content", Text);
            } else {
                eb.AddField("Message Content Unavailable", Text);
            }

            eb.WithFooter("Message was sent at " + MessageTimestamp);

            return eb.Build();
        }
    }
}
