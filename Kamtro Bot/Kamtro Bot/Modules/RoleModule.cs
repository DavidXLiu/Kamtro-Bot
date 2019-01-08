using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// Module made for commands that change user's roles.
    /// </summary>
    [Name("Role")]
    public class RoleModule : ModuleBase<SocketCommandContext>
    {
        [Command("addrole"), Alias("add", "give", "giveme", "give me")]
        [Name("AddRole")]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        [Summary("Adds an allowed role to the user, unless they already have it or are restricted from obtaining it.")]
        public async Task AddRoleAsync([Remainder]string message)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);

            // Find if user entered a role
            if (String.IsNullOrWhiteSpace(message))
            {
                /// TO DO
                /// When nothing is entered, make a prompt for selecting a role to add
                /// When a role cannot be added, notify the user that it is not a modifiable role
                /// When the user already has the role, notify the user that they already have the role
                /// Give the role to the user if they do not have the role
                /// Notify the user if the role doesn't exist
                /// 
                /// Make a roles command for listing all roles and which can be changed
            }
        }
    }
}
