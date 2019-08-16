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
            bool isAdmin = ServerData.HasPermissionLevel(Context.Guild.GetUser(Context.User.Id), ServerData.PermissionLevel.MODERATOR);

            HelpEmbed he = new HelpEmbed(Context, admin: isAdmin);
            await he.Display(Context.User.GetOrCreateDMChannelAsync().Result);
        }
    }
}
