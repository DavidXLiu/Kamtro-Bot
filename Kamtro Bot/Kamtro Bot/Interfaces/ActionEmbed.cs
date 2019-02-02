using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Kamtro_Bot.Handlers;

namespace Kamtro_Bot.Interfaces
{
    public abstract class ActionEmbed : KamtroEmbedBase
    {
        public SocketUser CommandCaller;

        public override async Task Display(ISocketMessageChannel channel) {
            await base.Display(channel);

            ReactionHandler.AddEvent(this, CommandCaller.Id);  // Add the embed to the event queue with the correct ID
        }

        public override async Task Display(IDMChannel channel) {
            await base.Display(channel);

            ReactionHandler.AddEvent(this, CommandCaller.Id);  // Add the embed to the event queue with the correct ID
        }
    }
}
