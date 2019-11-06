using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces.MessageEmbeds
{
    public class RewardEmbed : MessageEmbed
    {
        #region Input Fields
        [InputField("Color", 1, 1)]
        public string EmbedColor;

        [InputField("Message Text", 1, 2)]
        public string MessageText;

        [InputField("Kamtroken Reward", 1, 3)]
        public string KamtrokenReward;

        [InputField("Temporary Rep Reward", 1, 4)]
        public string TempRepReward;

        [InputField("Permanant Rep Reward", 1, 5)]
        public string PermRepReward;
        #endregion

        private bool InvalidNumberWarning = false;
        private bool GiftSent = false;

        SocketGuildUser Winner;

        public RewardEmbed(SocketCommandContext ctx, SocketGuildUser winner) {
            SetCtx(ctx);

            Winner = winner;

            RegisterMenuFields();
            AddMenuOptions(ReactionHandler.CHECK, ReactionHandler.DECLINE);
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch (action.Emote.ToString()) {
                case ReactionHandler.CHECK_STR:
                    // verify inputs
                    int k, t, p;  // kamtrokens, temp rep, and perm rep respectively
                    if(!(int.TryParse(KamtrokenReward, out k) && int.TryParse(TempRepReward, out t) && int.TryParse(PermRepReward, out p))) {
                        InvalidNumberWarning = true;
                        await UpdateEmbed();
                        return;
                    }

                    // Color
                    string c = EmbedColor.Replace("#", "").Replace("0x", "").Replace("x", "");
                    uint cc;

                    if(!uint.TryParse(c, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out cc)) {
                        InvalidNumberWarning = true;
                        await UpdateEmbed();
                        return;
                    }

                    Color ec = new Color(cc);

                    // Add the stats
                    UserDataNode data = UserDataManager.GetUserData(Winner);
                    data.Kamtrokens += k;
                    data.ReputationToGive += t;
                    data.MaxReputation += p;
                    UserDataManager.SaveUserData();

                    // Send the gift
                    string wins = "";
                    
                    if (k > 0) wins += $"{k} Kamtroken{(k == 1 ? "":"s")}, ";
                    if (t > 0) wins += $"{t} Temporary Reputation Point{(t == 1 ? "" : "s")}";
                    wins.TrimEnd(',', ' ');
                    if (p > 0) wins += $"\n\nIn addition, the amount of repuation you get per week has been increaced by {k}";

                    await BotUtils.DMUserAsync(Winner, new BasicEmbed("Congradulations!", $"You have won {(string.IsNullOrWhiteSpace(wins) ? "my appreciation!":wins)}", "Winnings", ec, "", MessageText).GetEmbed());
                    
                    // Update the embed
                    GiftSent = true;
                    await UpdateEmbed();
                    EventQueueManager.RemoveMessageEvent(this);
                    break;

                case ReactionHandler.DECLINE_STR:
                    EventQueueManager.RemoveMessageEvent(this);
                    await Message.DeleteAsync();
                    await Context.Channel.SendMessageAsync(BotUtils.KamtroText("Reward Sending Canceled."));
                    break;
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Award");
            eb.WithColor(BotUtils.Orange);

            if (!GiftSent) {
                eb.AddField("Rewarded User", $"{Winner.Mention}");
                eb.WithDescription("Award a user for winning a contest, or for any reason. This embed is a convinient way to give a user things packaged with a nice message sent directly to them.");

                AddEmbedFields(eb);

                // Warnings
                if (InvalidNumberWarning) {
                    InvalidNumberWarning = false;
                    eb.AddField("Warning", "One of the numbers you entered was either too large, or contained a non-digit character. Please check your inputs, it may also be an issue with an invalid color code!");
                }

                AddMenu(eb);
            } else {
                eb.WithDescription("Award Sent!");
            }

            return eb.Build();
        }
    }
}
