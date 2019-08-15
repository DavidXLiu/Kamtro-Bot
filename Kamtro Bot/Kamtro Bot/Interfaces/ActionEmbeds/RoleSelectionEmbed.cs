using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class RoleSelectionEmbed : ActionEmbed
    {
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

        private Func<SocketRole, Task> selectedAction;

        private List<SocketRole> RoleOptions;
        private List<string> Numbers;
        private string EmbedMessage;
        private IMessageChannel Channel;

        public RoleSelectionEmbed(List<SocketRole> roles, Func<SocketRole, Task> action, SocketGuildUser caller, string message = "There were too many roles with that name") {
            Numbers = new List<string>();
            RoleOptions = roles;
            EmbedMessage = message;
            CommandCaller = caller;

            List<MenuOptionNode> nodes = new List<MenuOptionNode>();

            if (RoleOptions.Count <= 10) {
                for (int i = 0; i < RoleOptions.Count; i++) {
                    Numbers.Add(NUMBERS[i]);  // Add to the reaction options
                    nodes.Add(new MenuOptionNode(NUMBERS[i], $"Select user {i + 1}"));  // Add the menu node
                }

                AddMenuOptions(nodes.GetRange(0, RoleOptions.Count).ToArray());  // Add the menu options
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Select a User");
            builder.WithColor(BotUtils.Kamtro);

            if (RoleOptions.Count > 10) {
                builder.WithDescription("That name is too vague! Try spelling it out more, or adding the tag at the end.");
                return builder.Build();
            }

            int i = 1;

            foreach (SocketRole role in RoleOptions) {
                builder.AddField($"{i}.", $"{role.Mention}");
                i++;
            }

            return builder.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            string number = option.Emote.ToString();

            if (NUMBERS.Contains(number)) {
                int chosenIndex = Numbers.IndexOf(number);

                if (chosenIndex >= 0 && chosenIndex <= 10) {
                    await selectedAction(RoleOptions[chosenIndex]); // Call the method passed in.
                }

                EventQueueManager.RemoveEvent(this); // Remove it from the queue
                if (Channel == null) return;
                await Channel.DeleteMessageAsync(Message);
            }
        }

        public override async Task Display(IMessageChannel channel = null) {
            Channel = channel;
            await base.Display(channel);
        }
    }
}
