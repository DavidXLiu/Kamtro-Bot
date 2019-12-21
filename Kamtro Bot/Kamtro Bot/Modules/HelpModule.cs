using Discord.Commands;
using Kamtro_Bot.Interfaces.ActionEmbeds;
using Kamtro_Bot.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// Help Command module, for commands related to help
    /// </summary>
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Alias("h", "helpme", "commands")]
        public async Task HelpCommandAsync() {
            bool isAdmin = ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN);
            bool isMod = ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.MODERATOR) || isAdmin;
            HelpEmbed he = new HelpEmbed(Context, isMod, isAdmin);
            await he.Display(Context.User.GetOrCreateDMChannelAsync().Result);
        }
    }
}
