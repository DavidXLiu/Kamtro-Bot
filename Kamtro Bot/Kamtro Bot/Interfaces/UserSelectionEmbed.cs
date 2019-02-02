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
    public class UserSelectionEmbed : ActionEmbed
    {
        // This seems like a bad way to do this but it's the only way I could
        // get a switch statement to work.
        private const string ONE = "\u0031\ufe0f\u20e3";
        private const string TWO = "\u0032\ufe0f\u20e3";
        private const string THREE = "\u0033️️\ufe0f\u20e3";
        private const string FOUR = "\u0034\ufe0f\u20e3";
        private const string FIVE = "\u0035\ufe0f\u20e3";
        private const string SIX = "\u0036\ufe0f\u20e3";
        private const string SEVEN = "\u0037\ufe0f\u20e3";
        private const string EIGHT = "\u0038\ufe0f\u20e3";
        private const string NINE = "\u0039\ufe0f\u20e3";
        private const string TEN = "\U0001f51f";

        private static readonly string[] NUMBERS = { ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN };

        private List<string> Numbers;

        private Func<SocketGuildUser, Task> selectedAction;

        private List<SocketGuildUser> UserOptions;
        private string EmbedMessage;

        public UserSelectionEmbed(List<SocketGuildUser> users, Func<SocketGuildUser, Task> action, string message = "There were multiple users with that name!") {
            Numbers = new List<string>();
            UserOptions = users;
            EmbedMessage = message;

            selectedAction = action;

            List<MenuOptionNode> nodes = new List<MenuOptionNode>();

            if (UserOptions.Count <= 10) {
                for (int i = 0; i < UserOptions.Count; i++) {
                    Numbers.Add(NUMBERS[i]);  // Add to the reaction options
                    nodes.Add(new MenuOptionNode(NUMBERS[i], $"Select user {i+1}"));  // Add the menu node
                }

                AddMenuOptions(nodes.ToArray());  // Add the menu options
            }
        }
        

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Select a User");
            builder.WithColor(Color.Blue);

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
            string number = option.ToString();

            if(NUMBERS.Contains(number)) {
                int chosenIndex = Numbers.IndexOf(number);

                if(chosenIndex >= 0 && chosenIndex <= 10) {
                    // if the index is valid
                    SocketGuildUser su = UserOptions[chosenIndex];

                    await selectedAction(su); // Call the method passed in.
                }
            }
        }
    }
}
