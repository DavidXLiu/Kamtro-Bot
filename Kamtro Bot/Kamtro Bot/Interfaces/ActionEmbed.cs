using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces
{
    /// <summary>
    /// Embed with reaction options
    /// </summary>
    /// <remarks>
    /// Class chain:
    /// 
    /// KamtroEmbedBase > ActionEmbed
    /// 
    /// This is probably going to be the most common interface. 
    /// </remarks>
    public abstract class ActionEmbed : KamtroEmbedBase
    {
        public SocketGuildUser CommandCaller;
        protected List<MenuOptionNode> MenuOptions;  // This should stay null. If there are no options, then it's value doesn't matter.
                                                     // This should be assigned in the constructor of the class.

        public TimeSpan Timeout = new TimeSpan(0, 10, 0);

        /// <summary>
        /// Full context for the embed
        /// </summary>
        /// <remarks>
        /// Oh boy this was needed for so long. Full context for everything
        /// </remarks>
        public SocketCommandContext Context;

        /// <summary>
        /// This method performs the interface's action for the option chosen by the user.
        /// </summary>
        /// -C
        /// <param name="option"></param>
        public abstract Task PerformAction(SocketReaction option);


        /// <summary>
        /// Adds the menu options at the bottom of the embed.
        /// </summary>
        /// -C
        /// <param name="embedBuilder"></param>
        /// <returns>The updated EmbedBuilder</returns>
        protected EmbedBuilder AddMenu(EmbedBuilder embedBuilder) {
            string footerText = "";
            MenuOptionNode option;

            for (int i = 0; i < MenuOptions.Count(); i++) {
                option = MenuOptions[i];
                footerText += $"{option.Icon} {option.Description} {((i != MenuOptions.Count - 1) ? "| " : "")}";  // There are 3 variables in this line, 
                                                                                                                   // The first two are self-explanitory, but the last one is
                                                                                                                   // A turnary operator that only places the | splitter char 
                                                                                                                   // If it's not at the last element.
            }

            embedBuilder.WithFooter(footerText);

            return embedBuilder;
        }


        /// <summary>
        /// Initializes the list of Menu options and fills it
        /// </summary>
        /// -C
        /// <param name="options">The different menu options and their descriptions</param>
        protected void AddMenuOptions(params MenuOptionNode[] options) {
            if (MenuOptions == null) {
                MenuOptions = new List<MenuOptionNode>();  // Initialize the list, but only if it's null.
            }

            MenuOptions.AddRange(options);  // add the menu options
        }

        /// <summary>
        /// Clears all menu options
        /// </summary>
        protected void ClearMenuOptions() {
            MenuOptions.Clear();
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
            if (MenuOptions == null) return; // if there are no options return null. Otherwise it will crash on the next line.

            foreach (MenuOptionNode node in MenuOptions) {  // For each menu option

                Emoji x = new Emoji(node.Icon);

                await Message.AddReactionAsync(x);  // Add the emoji as a reaction
            }
        }

        /// <summary>
        /// Display the message, menu options and all
        /// </summary>
        /// <returns></returns>
        /// <param name="channel">The channel to display the message in. If left empty, it will be displayed in the channel the command was called in</param>
        public override async Task Display(IMessageChannel channel = null, string message = "") {
            if (channel == null) channel = Context.Channel;

            await base.Display(channel, message);

            await AddReactions();  // Add the reactions

            EventQueueManager.AddEvent(this);  // Add the embed to the event queue with the correct ID
        }

        /// <summary>
        /// Sets all context variables such as CommandCaller
        /// </summary>
        /// <param name="ctx">The SocketCommandContext</param>
        protected void SetCtx(SocketCommandContext ctx) {
            Context = ctx;
            CommandCaller = ctx.User as SocketGuildUser;
        }

        /// <summary>
        /// Updates the embed, and all content within it.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateEmbed() {
            await Message.ModifyAsync(_msg => _msg.Embed = GetEmbed());
        }
    }
}
