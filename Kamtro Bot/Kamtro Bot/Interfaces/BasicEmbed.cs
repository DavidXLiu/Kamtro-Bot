using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Kamtro_Bot.Interfaces
{
    public class BasicEmbed : KamtroEmbedBase
    {
        public override Embed GetEmbed() {
            throw new NotImplementedException();
        }

        public override void PerformAction(SocketReaction option) {
            throw new NotImplementedException();
        }
    }
}
