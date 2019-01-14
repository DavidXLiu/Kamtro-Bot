using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

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
        protected bool HasActions = true;  // default is true, more interfaces have input than don't.
        protected MenuOptionNode[] MenuOptions;  // This should stay uninitialized. If there are no options, then it's value doesn't matter.
                                                 // This should be initialized in the constructor of the class.
        protected RestUserMessage Message;  // The message that the embed is in. This isn't a SocketUserMessage because that's what the SendMessageAsync method returns.
                                            // Both types can do the same things.

        /// <summary>
        /// This method performs the interface's action for the option chosen by the user.
        /// </summary>
        /// -C
        /// <param name="option"></param>
        public abstract void PerformAction(SocketReaction option);

        /// <summary>
        /// Builds and returns the Interface as an Embed
        /// </summary>
        /// -C
        /// <returns>The Embed object</returns>
        public abstract Embed GetEmbed();

        /// <summary>
        /// Adds the menu options to the message as reactions.
        /// </summary>
        /// <remarks>
        /// This method is pretty much identical for all of the embeds that require reactions,
        /// so that's why it's defined here.
        /// </remarks>
        /// <param name="message">The me</param>
        /// <returns></returns>
        public async Task AddReactions() {
            List<Emoji> reactions = new List<Emoji>();  // make a new List for the reactions
            
            foreach (MenuOptionNode node in MenuOptions) {  // For each menu option
                reactions.Add(new Emoji(node.Icon));  // Store the reactions in the list
            }

            await Message.AddReactionsAsync(reactions.ToArray());  // And add them.
        }

        /// <summary>
        /// Displays the embed and sets the Message variable.
        /// </summary>
        /// <param name="channel">The channel to send the message in</param>
        /// <returns></returns>
        public async Task Display(ISocketMessageChannel channel) {
            Embed kamtroEmbed = GetEmbed();  // The embed to send

            Message = await channel.SendMessageAsync(null, false, kamtroEmbed);  // send the embed in the message

            await AddReactions();  // Add the reactions
        }
    }
}
