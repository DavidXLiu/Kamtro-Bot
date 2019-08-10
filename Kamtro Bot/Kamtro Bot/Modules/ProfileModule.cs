using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Interfaces.BasicEmbeds;
using Kamtro_Bot.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Modules
{
    public class ProfileModule : ModuleBase<SocketCommandContext>
    {
        [Command("rep")]
        [Alias("giverep")]
        public async Task RepUserAsync([Remainder] string username = "") {
            TimeSpan ts = new TimeSpan(7, 0, 0, 0);
            ts -= DateTime.Now - DateTime.Now.LastSunday();

            if (!UserDataManager.CanAddRep(Context.User)) {
                await ReplyAsync(BotUtils.KamtroText($"You have no more reputation left to give! Resets in {ts.Days} day{((ts.Days == 1) ? "":"s")}, {ts.Hours} hour{((ts.Hours == 1) ? "" : "s")}, {ts.Minutes} minute{((ts.Minutes == 1) ? "" : "s")}, and {ts.Seconds} second{((ts.Seconds == 1) ? "" : "s")}"));
                return;
            }

            if (username == "") {
                int rep = UserDataManager.GetUserData(BotUtils.GetGUser(Context)).ReputationToGive;
                await ReplyAsync(BotUtils.KamtroText($"You have {rep} reputation point{(rep == 1 ? "":"s")} left to give. Resets in {ts.Days} day{((ts.Days == 1) ? "" : "s")}, {ts.Hours} hour{((ts.Hours == 1) ? "" : "s")}, {ts.Minutes} minute{((ts.Minutes == 1) ? "" : "s")}, and {ts.Seconds} second{((ts.Seconds == 1) ? "" : "s")}"));
                return;
            }

            List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

            if (users.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I can't find a user with that name, make sure the name is spelt correctly!"));
                return;
            } else if (users.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("Please be more specific! You can attach a discriminator if you need to (Username#1234)"));
                return;
            } else if (users.Count == 1) {
                await AddRep(users[0]);
            } else {
                UserSelectionEmbed use = new UserSelectionEmbed(users, AddRep, Context.Guild.GetUser(Context.User.Id));
                await use.Display(Context.Channel);
            }
        }

        [Command("profile")]
        [Alias("userprofile", "p")]
        public async Task ProfileAsync([Remainder] string username = "") {
            if(username == "") {
                // user's profile
                SocketGuildUser usr = Context.Guild.GetUser(Context.User.Id);

                ProfileEmbed pe = new ProfileEmbed(UserDataManager.GetUserData(usr), usr);
                await pe.Display(Context.Channel);
                return;
            }

            // else, the requested user's profile

            List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

            if (users.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I can't find a user with that name, make sure the name is spelt correctly!"));
                return;
            } else if (users.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("Please be more specific! You can attach a discriminator if you need to (Username#1234)"));
                return;
            } else if (users.Count == 1) {
                await Profile(users[0]);
            } else {
                UserSelectionEmbed use = new UserSelectionEmbed(users, Profile, Context.Guild.GetUser(Context.User.Id));
                await use.Display(Context.Channel);
            }
        }

        [Command("setprofilecolor")]
        [Alias("setprofilecolour", "spc", "setcolor", "setcolour")]
        public async Task SetProfileColorAsync([Remainder] string col = "") {
            if(col.Length < 1) {
                await ReplyAsync(BotUtils.KamtroText("You need to specify a color! Try !setprofilecolor #ffccaa or !setprofilecolor 234 44 120"));
                return;
            }

            List<string> args = col.Split(' ').ToList();

            if(args.Count == 1) {
                // it's a hex code
                if(col.Length < 6 || col.Length > 8) {
                    await ReplyAsync(BotUtils.KamtroText("You need to specify a valid hex code, or RGB values. For hex code, You can use ABCDEF, xABCDEF, #ABCDEF, or 0xABCDEF"));
                    return;
                }

                string hex;

                if(col.Length == 7) {
                    hex = col.Substring(1);
                } else if(col.Length == 8) {
                    hex = col.Substring(2);
                } else {
                    hex = col;
                }

                uint color;

                bool succ = uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.CurrentCulture,  out color);

                if(!succ) {
                    await ReplyAsync(BotUtils.KamtroText("You need to specify a valid hex code. Hex codes can only contain the digits 0-9, as well as letters A-F in any case."));
                    return;
                }

                Color c = new Color(color);

                UserDataManager.GetUserData(BotUtils.GetGUser(Context)).ProfileColor = c.RawValue;
                UserDataManager.SaveUserData();

                // The parameter in the ToString call is for formatting. The x means hexidecimal format.
                await ReplyAsync(embed: new BasicEmbed("Set Profile Color", $"({c.R}, {c.G}, {c.B})\n#{c.RawValue.ToString("x")}", "Your Profile Color has been set to", UserDataManager.GetUserData(BotUtils.GetGUser(Context)).GetColor()).GetEmbed());
            }
        }

        #region Helper Methods
        private async Task AddRep(SocketUser user) {
            if(user.Id == Context.User.Id) {
                await ReplyAsync(BotUtils.KamtroText("You can't give a repuation point to yourself!"));
            }

            UserDataManager.AddRep(Context.User, user);

            await ReplyAsync(BotUtils.KamtroText($"{Context.User.Username} has given a reputation point to {user.Username}"));
        }

        private async Task Profile(SocketGuildUser user) {
            ProfileEmbed pe = new ProfileEmbed(UserDataManager.GetUserData(user), user);
            await pe.Display(Context.Channel);
        }
        #endregion
    }
}
