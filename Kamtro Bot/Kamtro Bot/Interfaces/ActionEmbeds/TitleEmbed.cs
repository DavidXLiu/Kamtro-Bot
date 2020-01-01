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
        public const int MAX_TITLES_DISPLAYED = 10;

        private bool NoTitleNotify = false;
        private bool TitleEquipedNotify = false;

        private int Cursor = 0;
        private int Start = 0;
        private int? SelectedTitle = null;

        private List<Tuple<int, TitleNode>> Titles;

        private SocketGuildUser User;

        public TitleEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            Titles = AchievementManager.GetTitles();
            User = BotUtils.GetGUser(ctx);

            AddMenuOptions(ReactionHandler.SELECT, ReactionHandler.UP, ReactionHandler.DOWN, ReactionHandler.BACK);
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Titles");
            eb.WithColor(BotUtils.Kamtro);

            if (SelectedTitle == null) {
                // if the embed is on the home page AKA title list page
                eb.WithColor(BotUtils.Kamtro);

                string txt = "";

                if (AchievementManager.TitleCount() <= 0) {
                    txt = "Titles not available. Contact Caron or Arcy.";
                } else if (AchievementManager.TitleCount() <= MAX_TITLES_DISPLAYED) {
                    for (int i = 0; i < Titles.Count; i++) {
                        txt += MakeBold($"{(Cursor == i ? CustomEmotes.CursorAnimated : CustomEmotes.CursorBlankSpace)}{Titles[i].Item2.Name}", UserDataManager.HasTitle(User, Titles[i].Item1)) + "\n";
                    }
                } else {
                    if (Start != 0) txt += "***[...]***\n";

                    for (int i = Start; i <= Math.Min(Titles.Count-1, MAX_TITLES_DISPLAYED + Start); i++) {
                        // display titles starting from Start
                        txt += MakeBold($"{(Cursor == i ? CustomEmotes.CursorAnimated : CustomEmotes.CursorBlankSpace)}{Titles[i].Item2.Name}", UserDataManager.HasTitle(User, Titles[i].Item1)) + "\n";
                    }

                    if (Start < Titles.Count - 1 - MAX_TITLES_DISPLAYED) txt += "***[...]***";
                }

                eb.AddField("Title List", txt.TrimEnd('\n'));

                MenuOptions.Find((x) => x.Icon == ReactionHandler.SELECT_STR).Description = "Select";
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

                if (tn.PermRepReward > 0) add += $"Earning this title will increase the number of rep points you get each week by {tn.PermRepReward}!\n";
                if (SelectedTitle.Value == 0) add += "This title will add a special badge to your profile!";

                if(!string.IsNullOrWhiteSpace(add)) {
                    eb.AddField("Additional Rewards:", add.TrimEnd('\n'));
                }

                MenuOptions.Find((x) => x.Icon == ReactionHandler.SELECT_STR).Description = "Equip Title";
                
            }

            if(NoTitleNotify) {
                NoTitleNotify = false;
                eb.AddField("Alert", "You don't have this title unlocked!");
            }

            if(TitleEquipedNotify) {
                TitleEquipedNotify = false;
                eb.AddField("Success", "Title Equipped!");
            }

            AddMenu(eb);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            switch(option.Emote.ToString()) {
                case ReactionHandler.UP_STR:
                    if (SelectedTitle == null) await CursorUp();
                    break;

                case ReactionHandler.DOWN_STR:
                    if (SelectedTitle == null) await CursorDown();
                    break;

                case ReactionHandler.SELECT_STR:
                    if(SelectedTitle == null) {
                        SelectedTitle = Titles[Cursor].Item1;
                    } else {
                        if(UserDataManager.HasTitle(User, SelectedTitle.Value)) {
                            UserDataManager.EquipTitle(User, SelectedTitle.Value);
                            TitleEquipedNotify = true;
                        } else {
                            NoTitleNotify = true;
                        }
                    }

                    await UpdateEmbed();
                    break;

                case ReactionHandler.BACK_STR:
                    SelectedTitle = null;
                    await UpdateEmbed();
                    break;
            }
        }

        private async Task CursorUp() {
            if (SelectedTitle == null) {
                Cursor--;

                if (Cursor < 0) {
                    Cursor = Titles.Count() - 1;
                    Start = Titles.Count() - MAX_TITLES_DISPLAYED;
                } else if (Cursor - Start <= 1) {
                    Start--;
                }
            }
           
            await UpdateEmbed();
        }

        private async Task CursorDown() {
            if (SelectedTitle == null) {
                Cursor++;

                if (Cursor >= Titles.Count()) {
                    Cursor = 0;
                    Start = 0;
                } else if(Cursor - Start > MAX_TITLES_DISPLAYED-1) {
                    Start++;
                }
            }

            await UpdateEmbed();
        }

        private static string MakeBold(string s, bool b) {
            if(b) {
                return $"**{s}**";
            }

            return s;
        }
    }
}
