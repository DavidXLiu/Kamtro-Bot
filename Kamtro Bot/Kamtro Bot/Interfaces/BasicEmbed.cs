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
        public override Embed GetEmbed() {
            throw new NotImplementedException();
        }

        public new async Task PerformAction(SocketReaction option) {
            throw new NotImplementedException();
        }
    }
}
