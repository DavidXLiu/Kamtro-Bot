using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Interfaces.BasicEmbeds;
using Kamtro_Bot.Util;
using Kamtro_Bot.Nodes;

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
            SocketGuildUser user = BotUtils.GetGUser(Context);

            if (user.GuildPermissions.Administrator || ServerData.IsAdmin(user) || ServerData.IsModerator(user)) {
                await Hacked();
            }
        }

        [Command("offline")]
        [Alias("off", "sleep")]
        public async Task OfflineAsync() {
            SocketGuildUser user = BotUtils.GetGUser(Context);
            if (ServerData.HasPermissionLevel(user, ServerData.PermissionLevel.MODERATOR)) {
                await ReplyAsync(BotUtils.KamtroText("Goodnight 💤"));
                await Program.Client.LogoutAsync();
            }
        }

        [Command("crossbanscan")]
        [Alias("cbs", "xbs", "xscan")]
        public async Task GenerateCrossBansAsync() {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // permissions checking

            ScanInfoEmbed scan = new ScanInfoEmbed();

            await ReplyAsync(BotUtils.KamtroText("Scanning Kamexico for Bans..."));
             
            foreach(RestBan ban in await ServerData.Kamexico.GetBansAsync()) {
                scan.KamexTotal++;
                scan.KamexUnique++; 
                scan.Total++;

                GeneralHandler.CrossBan[ban.User.Id] = new CrossBanDataNode(1, ban.Reason);
            }

            await ReplyAsync(BotUtils.KamtroText("Scanning Kamexico for Bans..."));

            foreach (RestBan ban in await ServerData.Retropolis.GetBansAsync()) {
                scan.RetroTotal++;
                scan.Total++;

                if (GeneralHandler.CrossBan.ContainsKey(ban.User.Id)) {
                    // if it's a mutual ban
                    scan.Mutual++;
                    scan.KamexUnique--;
                } else {
                    // else if it's unique to retropolis
                    scan.RetroUnique++;
                }
            }

            int errcount = 0;
             
            foreach(ulong id in GeneralHandler.CrossBan.Keys) {
                if(ServerData.Server.GetUser(id) != null) {
                    bool sent = await BotUtils.DMUserAsync(ServerData.Server.GetUser(id), new BanNotifyEmbed($"You were banned on {GeneralHandler.CrossBan[id].GetServer()}, and therefore have been auto banned from Kamtro.\nOld ban reason:\n\n{GeneralHandler.CrossBan[id].Reason}").GetEmbed());

                    if (!sent) errcount++;
                    
                    await ServerData.Server.AddBanAsync(ServerData.Server.GetUser(id));
                    scan.InKamtro++;
                }
            }

            if (errcount > 0) await ReplyAsync(BotUtils.KamtroText($"{errcount} users could not be messaged."));
            GeneralHandler.SaveList();

            await scan.Display(ServerData.AdminChannel);
        }

        private async Task Hacked() {
            // leave all servers
            Console.WriteLine("Bot has been hacked! Leaving all servers...");

            await ReplyAsync(BotUtils.KamtroText("Arrivederci."));

            foreach (SocketGuild server in Program.Client.Guilds) {
                await server.LeaveAsync();

                Console.WriteLine($"Left {server.Name} [ID: {server.Id}]");
            }
        }
    }
}
