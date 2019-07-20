using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Handlers
{
    public class GeneralHandler
    {
        public GeneralHandler(DiscordSocketClient client) {
            client.GuildMemberUpdated += HandleMemberUpdate;
        }

        public async Task HandleMemberUpdate(SocketGuildUser before, SocketGuildUser after) {
            if (before.Guild != ServerData.Server) return; // If it's not on kamtro, ignore it

            if(BotUtils.GetFullUsername(before) != BotUtils.GetFullUsername(after)) {
                // If the user changed their name.
            }

            if(before.Roles.Count != after.Roles.Count) {
                // role update
                foreach(SocketRole role in after.Roles) {
                    if(ServerData.SilencedRoles.Contains(role)) {
                        // remove mature role if user has it
                        if(after.Roles.Contains(ServerData.NSFWRole)) {
                            await after.RemoveRoleAsync(ServerData.NSFWRole);
                        }
                    }
                }
            }
        }
    }
}
