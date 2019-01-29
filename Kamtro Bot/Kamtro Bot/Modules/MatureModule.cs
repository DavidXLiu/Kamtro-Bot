using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Managers;

using Kamtro_Bot.Util;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// This is the module that handels the Mature and NSFW commands
    /// </summary>
    [Name("Mature")]
    public class MatureModule : ModuleBase<SocketCommandContext>
    {
        private string[] BlacklistAliases = { "blacklist", "deny", "nomature", "removemature", "nomoreporn" };

        [Command("blacklist"), Alias("deny","nomature","removemature","nomoreporn")]
        [Name("Blacklist")]
        [Summary("Puts a user on the blacklist list for mature, preventing them from getting access to it through the bot.")]
        /// Arcy
        public async Task BlacklistAsync([Remainder]string message)
        {
            List<SocketGuildUser> users = BotUtils.GetUser(Context.Message, UtilStringComparison.FindAlias(Context.Message.Content, BlacklistAliases));

            // Can't find user
            if (users.Count == 0)
            {
                await ReplyAsync(BotUtils.KamtroText("I could not find that user. You can choose users by mention, username, nickname, or Discord ID."));
                return;
            }
            // Too vague
            else if (users.Count > 10)
            {
                await ReplyAsync(BotUtils.KamtroText("That name is too vague. Please try specifying the name or using their Discord ID or mention."));
                return;
            }
            else if (users.Count == 1)
            {

            }
        }
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
    }
}
