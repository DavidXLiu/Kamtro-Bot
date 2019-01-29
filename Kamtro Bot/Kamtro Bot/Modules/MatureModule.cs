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

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// This is the module that handels the Mature and NSFW commands
    /// </summary>
    [Name("Mature")]
    public class MatureModule : ModuleBase<SocketCommandContext>
    {
        private const string USER_ID_REGEX = "\\d+";

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
            
        }
    }
}
