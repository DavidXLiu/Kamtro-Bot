using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// This is the Admin module, only those marked as admins in the config may use these commands.
    /// </summary>
    [Name("Admin")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("hacked")]
        public async Task HackedAsync() {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);

            if (user.GuildPermissions.Administrator || ServerData.IsAdmin(user) || ServerData.IsModerator(user)) {
                HackedEmbed he = new HackedEmbed(user);
                await he.Display(Context.Channel);
            }
        }

        [Command("offline")]
        [Alias("off", "sleep")]
        public async Task OfflineAsync() {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            if (ServerData.HasPermissionLevel(user, ServerData.PermissionLevel.MODERATOR)) {
                await ReplyAsync(BotUtils.KamtroText("Goodnight 💤"));
                await Program.Client.LogoutAsync();
                Environment.Exit(0);
            }
        }
    }
}
