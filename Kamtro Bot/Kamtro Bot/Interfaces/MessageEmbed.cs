using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kamtro_Bot.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Interfaces
{
    /// <summary>
    /// This is a abse class for embeds that are waiting on a message
    /// </summary>
    public abstract class MessageEmbed : ActionEmbed
    {
        public SocketChannel CommandChannel;

        public abstract Task OnMessage(SocketUserMessage message);

        /// <summary>
        /// This is the method that will be called when the user sends a message in the bot channel if the interface is waiting on a message.
        /// </summary>
        /// <remarks>
        /// It is guaranteed that the message is from the correct user in the correct channel when this is called.
        /// </remarks>
        /// -C
        /// <param name="message">The message that was sent by the user</param>
        public abstract void PerformMessageAction(SocketUserMessage message);


        public virtual async Task AddInputsAsync(EmbedBuilder builder) {

        }

        /// <summary>
        /// Displays the embed.
        /// </summary>
        /// <param name="channel">The channel to display the embed in</param>
        /// <returns></returns>
        public override async Task Display(IMessageChannel channel) {
            await base.Display(channel);

            EventQueueManager.AddMessageEvent(this);
        }
    }
}
