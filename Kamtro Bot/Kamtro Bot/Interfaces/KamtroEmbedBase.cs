using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces
{
    /// <summary>
    /// This is an abstract base class for all other Kamtro embeds.
    /// For an embed that is just text, see <see cref="BasicEmbed"/>
    /// </summary>
    /// -C
    public abstract class KamtroEmbedBase
    {
        protected bool HasActions = true;  // default is true, more interfaces have input than don't.
        protected MenuOptionNode[] MenuOptions;  // This should stay uninitialized. If there are no options, then it's value doesn't matter.
                                                 // This should be initialized in the constructor of the class.

        /// <summary>
        /// This method performs the interface's action for the option chosen by the user.
        /// </summary>
        /// <param name="option"></param>
        public abstract void PerformAction(SocketReaction option);

        // This method returns the interface as an embed object.
        public abstract Embed GetEmbed();

        public async Task AddReactions(SocketUserMessage message) {
            List<Emoji> reactions = new List<Emoji>();  // make a new List for the reactions
            
            foreach (MenuOptionNode node in MenuOptions) {  // For each menu option
                reactions.Add(new Emoji(node.Icon));  // Store the reactions in the list
            }

            await message.AddReactionsAsync(reactions.ToArray());  // And add them.
        }
    }
}
