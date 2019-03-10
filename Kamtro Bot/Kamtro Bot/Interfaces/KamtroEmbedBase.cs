using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces
{
    /// <summary>
    /// This is an abstract base class for all other Kamtro embeds.
    /// </summary>
    /// <remarks>
    /// For an embed that is just text, see <see cref="BasicEmbed"/>.
    /// 
    /// The embed does not automatically add itself to the pending action dictionary, and it should never do so.
    /// It's much safer and easier to manually add it when it is created, and in the context in which it is created,
    /// such as inside the command method.
    /// </remarks>
    /// -C
    public abstract class KamtroEmbedBase
    {
        public RestUserMessage Message;  // The message that the embed is in. This isn't a SocketUserMessage because that's what the SendMessageAsync method returns.
                                         // Both types can do the same things.

        /// <summary>
        /// Builds and returns the Interface as an Embed
        /// </summary>
        /// -C
        /// <returns>The Embed object</returns>
        public abstract Embed GetEmbed();

        /// <summary>
        /// Displays the embed and sets the Message variable.
        /// </summary>
        /// <remarks>
        /// Woo turns out there was a common interface!
        /// No more duplicate methods!
        /// </remarks>
        /// <param name="channel">The channel to send the message in</param>
        /// <returns></returns>
        public async virtual Task Display(IMessageChannel channel = null) {
            Embed kamtroEmbed = GetEmbed();  // The embed to send

            channel = channel ?? Message.Channel;  // If there is no channel specified, pass in the one that the embed came from

            Message = await channel.SendMessageAsync(null, false, kamtroEmbed) as RestUserMessage;  // send the embed in the message
        }
    }
}
