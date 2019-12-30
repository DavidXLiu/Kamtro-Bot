using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class SuggestionEmbed : KamtroEmbedBase
    {
        private SocketGuildUser User;
        private string Suggestion;

        public SuggestionEmbed(SocketGuildUser user, string suggestion) {
            User = user;
            Suggestion = suggestion;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Suggestion");
            eb.WithColor(BotUtils.Kamtro);
            eb.WithThumbnailUrl(User.GetAvatarUrl());

            eb.AddField($"The user {BotUtils.GetFullUsername(User)} has suggested:", Suggestion);

            return eb.Build();
        }
    }
}
