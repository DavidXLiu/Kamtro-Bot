using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Util
{
    public class ItemSelectionEmbed<T> : ActionEmbed
    {
        #region Constants
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
        #endregion

        private string Title;
        private string TooVagueMessage;
        private string ItemMentionField;

        private bool MentionIsMethod;

        private Func<T, Task> SelectedAction;
        private List<T> Options;
        private List<string> Numbers = null;

        public ItemSelectionEmbed(SocketCommandContext ctx, Func<T, Task> action, List<T> options, string title, string error = "That name is too vague. Please be more specific!", string itemMentionField = "ToString", bool mentionIsMethod = true) {
            SelectedAction = action;
            Options = options;
            Title = title;
            TooVagueMessage = error;
            ItemMentionField = itemMentionField;
            MentionIsMethod = mentionIsMethod;

            SetCtx(ctx);

            if(Options.Count <= 10) {
                List<MenuOptionNode> nodes = new List<MenuOptionNode>();
                Numbers = new List<string>();

                for (int i = 0; i < Options.Count; i++) {
                    Numbers.Add(NUMBERS[i]);  // Add to the reaction options
                    nodes.Add(new MenuOptionNode(NUMBERS[i], $"Select role {i + 1}"));  // Add the menu node
                }

                AddMenuOptions(nodes.GetRange(0, Options.Count).ToArray());
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle(Title);
            builder.WithColor(BotUtils.Kamtro);

            if (Options.Count > 10) {
                builder.WithDescription(TooVagueMessage);
                return builder.Build();
            }

            int i = 1;

            foreach (T option in Options) {
                if(MentionIsMethod) {
                    builder.AddField($"{i}.", $"{option.GetType().GetMethod(ItemMentionField).Invoke(option, null) as string}");
                } else {
                    builder.AddField($"{i}.", $"{option.GetType().GetField(ItemMentionField).GetValue(option) as string}");
                }
                
                i++;
            }

            return builder.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            string number = option.Emote.ToString();

            if (NUMBERS.Contains(number)) {
                int chosenIndex = Numbers.IndexOf(number);

                if (chosenIndex >= 0 && chosenIndex <= 10) {
                    await SelectedAction(Options[chosenIndex]); // Call the method passed in.
                }

                EventQueueManager.RemoveEvent(this); // Remove it from the queue
                if (Context.Channel == null) return;
                await Context.Channel.DeleteMessageAsync(Message);
            }
        }
    }
}
