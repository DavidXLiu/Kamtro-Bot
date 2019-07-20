using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces
{
    public class UserSelectionEmbed : ActionEmbed
    {
        // This seems like a bad way to do this but it's the only way I could
        // get a switch statement to work.
        private const string ONE = "1⃣";
        private const string TWO = "2⃣";
        private const string THREE = "3⃣";
        private const string FOUR = "4⃣";
        private const string FIVE = "5⃣";
        private const string SIX = "6⃣";
        private const string SEVEN = "7⃣";
        private const string EIGHT = "8⃣";
        private const string NINE = "9⃣";
        private const string TEN = "\U0001f51f";

        private static readonly string[] NUMBERS = { ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN };
        
        private Func<SocketGuildUser, Task> selectedAction;
        private Func<SocketGuildUser, SocketCommandContext, Task> selectedActionWithContext;

        private List<SocketGuildUser> UserOptions;
        private List<string> Numbers;

        private Color EmbedColor = Color.Blue;

        private bool hasContext;
        private string EmbedMessage;

        public UserSelectionEmbed(List<SocketGuildUser> users, Func<SocketGuildUser, Task> action, SocketGuildUser caller, string message = "There were multiple users with that name!") {
            Numbers = new List<string>();
            UserOptions = users;
            EmbedMessage = message;
            CommandCaller = caller;

            selectedAction = action;

            hasContext = false;

            List<MenuOptionNode> nodes = new List<MenuOptionNode>();

            if (UserOptions.Count <= 10) {
                for (int i = 0; i < UserOptions.Count; i++) {
                    Numbers.Add(NUMBERS[i]);  // Add to the reaction options
                    nodes.Add(new MenuOptionNode(NUMBERS[i], $"Select user {i+1}"));  // Add the menu node
                }

                AddMenuOptions(nodes.GetRange(0, UserOptions.Count).ToArray());  // Add the menu options
            }
        }

        public UserSelectionEmbed(List<SocketGuildUser> users, Func<SocketGuildUser, SocketCommandContext, Task> action, SocketCommandContext context, string message = "There were multiple users with that name!") {
            Numbers = new List<string>();
            UserOptions = users;
            EmbedMessage = message;
            CommandCaller = context.User as SocketGuildUser;
            Context = context;

            selectedActionWithContext = action;

            hasContext = true;

            List<MenuOptionNode> nodes = new List<MenuOptionNode>();

            if (UserOptions.Count <= 10) {
                for (int i = 0; i < UserOptions.Count; i++) {
                    Numbers.Add(NUMBERS[i]);  // Add to the reaction options
                    nodes.Add(new MenuOptionNode(NUMBERS[i], $"Select user {i + 1}"));  // Add the menu node
                }

                nodes.Add(ReactionHandler.DONE);
                
                AddMenuOptions(nodes.GetRange(0, UserOptions.Count+1).ToArray());  // Add the menu options
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Select a User");
            builder.WithColor(EmbedColor);

            if (UserOptions.Count > 10) {
                builder.WithDescription("That name is too vague! Try spelling it out more, or adding the tag at the end.");
                return builder.Build();
            }

            int i = 1;

            foreach (SocketGuildUser user in UserOptions) {
                builder.AddField($"{i}.", $"{user.Username}#{user.Discriminator}");
                i++;
            }

            return builder.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            string number = option.Emote.ToString();

            if(NUMBERS.Contains(number)) {
                int chosenIndex = Numbers.IndexOf(number);

                if(chosenIndex >= 0 && chosenIndex <= 10) {
                    // if the index is valid
                    SocketGuildUser su = UserOptions[chosenIndex];

                    if(hasContext) {
                        // If this is the type of embed that needs context
                        await selectedActionWithContext(su, Context); // Call the context method
                    } else {
                        await selectedAction(su); // Call the method passed in.
                    }

                    // EventQueueManager.RemoveEvent(this, Context.User.Id); // Remove it from the queue
                }
            }
        }

        public void SetColor(uint color) {
            EmbedColor = new Color(color);
        }
    }
}
