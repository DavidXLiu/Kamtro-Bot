using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Kamtro_Bot.Handlers;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Managers;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// This is the module that handels the Mature and NSFW commands
    /// </summary>  
    [Name("Mature")]
    public class MatureModule : ModuleBase<SocketCommandContext>
    {
        private static Regex UserMention = new Regex(@"<!*\d+>");
        private static Regex FullUsername = new Regex(@".+#\d\d\d\d\s+$");

        [Command("mature")]
        [Alias("nsfw", "lewd")]
        public async Task NSFWCommandAsync() {
            if(UserDataManager.UserData[Context.Message.Author.Id].Nsfw) {
                NSFWEmbed nsfw = new NSFWEmbed(Context.User as SocketGuildUser);
                await nsfw.Display(await Context.User.GetOrCreateDMChannelAsync());
                ReactionHandler.AddEvent(nsfw, Context.Message.Author.Id);
            } else {
                await ReplyAsync(BotUtils.KamtroText("Nice try ( ͡° ͜ʖ ͡°)"));
            }
        }
        
        [Command("matureblacklist")]
        [Alias("mbl", "nsfwbl", "nonsfw")]
        public async Task BlacklistNSFWAsync([Remainder] string args) {
            List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

            if(users.Count == 0) {
                // if no users are mentioned
                await ReplyAsync(BotUtils.KamtroText("You need to specify a user!"));
            } else if(users.Count == 1) {
                // If a user is mentioned
                UserDataManager.AddUserIfNotExists(users[0]);  // Make sure the user has a node
                await NoNSFWAsync(users[0]);
            } else {
                // More than one user mentioned, or ambiguous user
                UserSelectionEmbed use = new UserSelectionEmbed(users, NoNSFWAsync, Context);
                await use.Display(Context.Channel);
            }
        }

        public async Task NoNSFWAsync(SocketGuildUser user) {
            UserDataManager.UserData[user.Id].Nsfw = false;
            await ReplyAsync(BotUtils.KamtroText($"User {user.Username} is now blacklisted from #mature"));
        }

        public async Task NoNSFWAsync(SocketGuildUser user, SocketCommandContext ctx) {
            UserDataManager.UserData[user.Id].Nsfw = false;
            await ctx.Channel.SendMessageAsync(BotUtils.KamtroText($"User {user.Username} is now blacklisted from #mature"));
        }
    }
}
