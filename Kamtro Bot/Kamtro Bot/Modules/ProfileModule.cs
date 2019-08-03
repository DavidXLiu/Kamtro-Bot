using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Managers;
using System;
using System.Collections.Generic;
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
            if(!UserDataManager.CanAddRep(Context.User)) {
                TimeSpan ts = new TimeSpan(1, 0, 0, 0);

                ts -= DateTime.Now - DateTime.Now.LastSunday();

                await ReplyAsync(BotUtils.KamtroText($"You have no more reputation left to give! Resets in {ts.Days} day{((ts.Days == 1) ? "":"s")}, {ts.Hours} hour{((ts.Hours == 1) ? "" : "s")}, {ts.Minutes} minute{((ts.Minutes == 1) ? "" : "s")}, and {ts.Seconds} second{((ts.Seconds == 1) ? "" : "s")}"));
                return;
            }

            if (username == "") {
                await ReplyAsync(BotUtils.KamtroText("Please specify a user!"));
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

        #region Helper Methods
        private async Task AddRep(SocketUser user) {
            if(user.Id == Context.User.Id) {
                await ReplyAsync(BotUtils.KamtroText("You can't give a repuation point to yourself!"));
            }

            UserDataManager.AddRep(Context.User, user);

            await ReplyAsync(BotUtils.KamtroText($"{Context.User.Username} has given a reputation point to {user.Username}"));
        }
        #endregion
    }
}
