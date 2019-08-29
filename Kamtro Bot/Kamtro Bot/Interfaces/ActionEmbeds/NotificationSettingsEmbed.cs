using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class NotificationSettingsEmbed : ActionEmbed {

        public NotificationSettingsEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);
        }

        public override Embed GetEmbed() {
            throw new NotImplementedException();
        }

        public override Task PerformAction(SocketReaction option) {
            throw new NotImplementedException();
        }
    }
}
