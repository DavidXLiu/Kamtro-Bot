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
        protected bool HasActions = true;  // default is true, more interfaces have input than don't.
        protected List<MenuOptionNode> MenuOptions;  // This should stay uninitialized. If there are no options, then it's value doesn't matter.
                                                 // This should be initialized in the constructor of the class.
        public RestUserMessage Message;  // The message that the embed is in. This isn't a SocketUserMessage because that's what the SendMessageAsync method returns.
                                         // Both types can do the same things.


        /// <summary>
        /// This method performs the interface's action for the option chosen by the user.
        /// </summary>
        /// -C
        /// <param name="option"></param>
        public abstract Task PerformAction(SocketReaction option);

        /// <summary>
        /// Builds and returns the Interface as an Embed
        /// </summary>
        /// -C
        /// <returns>The Embed object</returns>
        public abstract Embed GetEmbed();

        /// <summary>
        /// This is the method that will be called when the user sends a message in the bot channel if the interface is waiting on a message.
        /// </summary>
        /// <remarks>
        /// This method is marked as virtual because not all interfaces will be waiting on a message, and therefore
        /// not all interfaces actually need this method. 
        /// -C
        /// </remarks>
        /// <param name="message">The message that was sent by the user</param>
        public virtual void PerformMessageAction(SocketUserMessage message) {
            // THIS IS EMPTY BY DEFAULT.
        }


        /// <summary>
        /// Initializes the list of Menu options and fills it
        /// </summary>
        /// -C
        /// <param name="options">The different menu options and their descriptions</param>
        protected void AddMenuOptions(params MenuOptionNode[] options) {
            MenuOptions = new List<MenuOptionNode>();  // Initialize the list
            MenuOptions.AddRange(options);  // Fill it with the menu options
        }

        /// <summary>
        /// Adds the menu options at the bottom of the embed.
        /// </summary>
        /// -C
        /// <param name="embedBuilder"></param>
        /// <returns>The updated EmbedBuilder</returns>
        protected EmbedBuilder AddMenu(EmbedBuilder embedBuilder) {
            string footerText = "";
            MenuOptionNode option;

            for(int i = 0; i < MenuOptions.Count(); i++) {
                option = MenuOptions[i];
                footerText += $"{option.Icon} {option.Description} {((i != MenuOptions.Count-1) ? "| ":"")}";  // There are 3 variables in this line, 
                                                                                                               // The first two are self-explanitory, but the last one is
                                                                                                               // A turnary operator that only places the | splitter char 
                                                                                                               // If it's not at the last element .
            }

            embedBuilder.WithFooter(footerText);

            return embedBuilder;
        }

        /// <summary>
        /// Adds the menu options to the message as reactions.
        /// </summary>
        /// <remarks>
        /// This method is pretty much identical for all of the embeds that require reactions,
        /// so that's why it's defined here.
        /// </remarks>
        /// <returns></returns>
        public async Task AddReactions() {            
            foreach (MenuOptionNode node in MenuOptions) {  // For each menu option

                Emoji x = new Emoji(node.Icon);
                
                await Message.AddReactionAsync(x);  // Add the emoji as a reaction
            }
        }

        /// <summary>
        /// Displays the embed and sets the Message variable.
        /// </summary>
        /// <param name="channel">The channel to send the message in</param>
        /// <returns></returns>
        public async virtual Task Display(ISocketMessageChannel channel) {
            Embed kamtroEmbed = GetEmbed();  // The embed to send

            Message = await channel.SendMessageAsync(null, false, kamtroEmbed) as RestUserMessage;  // send the embed in the message
           
            if(HasActions) {
                await AddReactions();  // Add the reactions
            }
        }

        /// <summary>
        /// Displays the embed and sets the Message variable.
        /// </summary>
        /// <remarks>
        /// As you may have noticed, this is literally just a copy-paste overload of the above method.
        /// Why would I do this? Because there is no common interface that you get for a Text Channel and a DM. There are common classes sure,
        /// but no useful methods return those so  ;( i cri
        /// 
        /// -C
        /// </remarks>
        /// <param name="channel">The channel to send the message in</param>
        /// <returns></returns>
        public async virtual Task Display(IDMChannel channel) {
            Embed kamtroEmbed = GetEmbed();  // The embed to send

            Message = await channel.SendMessageAsync(null, false, kamtroEmbed) as RestUserMessage;  // send the embed in the message

            if (HasActions) {
                await AddReactions();  // Add the reactions
            }
        }
    }
}
