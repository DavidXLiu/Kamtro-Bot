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

        /// <summary>
        /// This method performs the interface's action for the option chosen by the user.
        /// </summary>
        /// -C
        /// <param name="option"></param>
        public abstract Task PerformAction(SocketReaction option);

        public override async Task Display(IMessageChannel channel) {
            await base.Display(channel);

            await AddReactions();  // Add the reactions

            EventQueueManager.AddEvent(this);  // Add the embed to the event queue with the correct ID
        }
    }
}
