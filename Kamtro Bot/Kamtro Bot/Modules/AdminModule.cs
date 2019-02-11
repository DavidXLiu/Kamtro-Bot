﻿using System;
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
                await Hacked();
            }
        }

        [Command("offline")]
        [Alias("off", "sleep")]
        public async Task OfflineAsync() {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            if (ServerData.HasPermissionLevel(user, ServerData.PermissionLevel.MODERATOR) || user.Id == 118892308086128641) {
                await ReplyAsync(BotUtils.KamtroText("Goodnight 💤"));
                await Program.Client.LogoutAsync();
            }
        }

        private async Task Hacked() {
            // leave all servers
            Console.WriteLine("Bot has been hacked! Leaving all servers...");

            foreach (SocketGuild server in Program.Client.Guilds) {
                await server.LeaveAsync();

                Console.WriteLine($"Left {server.Name} [ID: {server.Id}]");
            }
        }
    }
}
