using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Kamtro_Bot.Interfaces
{
    /// <summary>
    /// Basic embed. Only text, no menu options.
    /// </summary>
    /// <remarks>
    /// TODO: THIS
    /// </remarks>
    /// -C
    public class BasicEmbed : KamtroEmbedBase
    {
        public string Title;

        public BasicEmbed() {
            HasActions = false;
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle(Title);

            return builder.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            // NO-OP
        }
    }
}
