using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class TitleEmbed : ActionEmbed
    {
        public const int MAX_TITLES_DISPLAYED = 5;

        private int Cursor = 0;
        private int Start = 0;
        private int? SelectedTitle = null;

        private SocketGuildUser User;

        public TitleEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            User = BotUtils.GetGUser(ctx);

            AddMenuOptions(ReactionHandler.UP, ReactionHandler.DOWN, ReactionHandler.SELECT, ReactionHandler.BACK);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Titles");
            eb.WithColor(BotUtils.Kamtro);

            string txt = "";

            if (SelectedTitle != null) {
                // if the embed is on the home page AKA title list page
                eb.WithColor(BotUtils.Kamtro);

                if (AchievementManager.TitleCount() <= 0) {
                    txt = "Titles not available. Contact Caron or Arcy.";
                } else if (AchievementManager.TitleCount() <= MAX_TITLES_DISPLAYED) {
                    List<Tuple<int, TitleNode>> titles = AchievementManager.GetTitles();

                    for (int i = 0; i < titles.Count; i++) {
                        txt += MakeBold($"{(Cursor == i ? CustomEmotes.CursorAnimated : CustomEmotes.CursorBlankSpace)}{titles[i].Item2.Name}\n", UserDataManager.HasTitle(User, titles[i].Item1));
                    }
                } else {
                    List<Tuple<int, TitleNode>> titles = AchievementManager.GetTitles();

                    for (int i = Start; i < titles.Count + Start - 1; i++) {
                        // display titles starting from Start
                        txt += MakeBold($"{(Cursor == i ? CustomEmotes.CursorAnimated : CustomEmotes.CursorBlankSpace)}{titles[i].Item2.Name}\n", UserDataManager.HasTitle(User, titles[i].Item1));
                    }
                }
            } else {
                // if it's on a title page
                TitleNode tn = AchievementManager.GetTitle(SelectedTitle.Value);

                eb.WithColor(tn.GetColor());

                eb.AddField("Title Name", tn.Name);
                if (tn.Secret) eb.AddField("Secret", "This is a secret title!");

                eb.AddField("Kamtroken Reward", $"{tn.KamtrokenReward}", true);
                eb.AddField("Temp Rep Reward", $"{tn.TempRepReward}", true);

                // section for additional rewards
                // most common of these is the perm rep increase
                // Kamtro god will also pimp out your profile
                string add = "";

                if (tn.PermRepReward >= 0) add += $"Earning this title will increase the number of rep points you get each week by {tn.PermRepReward}!\n";
                if (SelectedTitle.Value == 0) add += "This title will add a special badge to your profile!";

                if(!string.IsNullOrWhiteSpace(add)) {
                    eb.AddField("Additional Rewards:", add.TrimEnd('\n'));
                }
            }

            eb.AddField("Title List", txt.TrimEnd('\n'));

            AddMenu(eb);

            return eb.Build();
        }

        public override Task PerformAction(SocketReaction option) {
            throw new NotImplementedException();
        }

        private static string MakeBold(string s, bool b) {
            if(b) {
                return $"**{s}**";
            }

            return s;
        }
    }
}
