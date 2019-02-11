using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Interfaces
{
    public abstract class ActionEmbed : KamtroEmbedBase
    {
        public SocketGuildUser CommandCaller;

        public override async Task Display(ISocketMessageChannel channel) {
            await base.Display(channel);

            EventQueueManager.AddEvent(this);  // Add the embed to the event queue with the correct ID
        }

        public override async Task Display(IDMChannel channel) {
            await base.Display(channel);

            EventQueueManager.AddEvent(this);  // Add the embed to the event queue with the correct ID
        }
    }
}
