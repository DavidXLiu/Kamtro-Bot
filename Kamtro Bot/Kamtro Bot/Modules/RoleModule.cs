using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

using Kamtro_Bot.Util;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// Module made for commands that change user's roles.
    /// </summary>
    [Name("Role")]
    public class RoleModule : ModuleBase<SocketCommandContext>
    {
        public static Dictionary<string, ulong> RoleMap; // The dictionary that will replace the two above variables.

        [Command("addrole"), Alias("add", "give", "giveme", "give me")]
        [Name("AddRole")]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        [Summary("Adds an allowed role to the user, unless they already have it or are restricted from obtaining it.")]
        public async Task AddRoleAsync([Remainder]string message)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);

            // Find if user entered a role
            if (string.IsNullOrWhiteSpace(message))
            {
                /// TO DO 
                /// When nothing is entered, make a prompt for selecting a role to add
                /// When a role cannot be added, notify the user that it is not a modifiable role
                /// When the user already has the role, notify the user that they already have the role
                /// Give the role to the user if they do not have the role
                /// Notify the user if the role doesn't exist
                /// 
                /// Make a roles command for listing all roles and which can be changed

                
            } else {
                bool hasRole = false;

                foreach (KeyValuePair<string, ulong> role in RoleMap) {  // for each possibility to what they meant for a role
                    if (UtilStringComparison.CompareWordScore(message, role.Key.ToLower()) > 0.66) {  // If they were close enough to a role
                        SocketGuildUser sender = Context.User as SocketGuildUser;  // Take the user who wants the role
                        SocketRole roleToAdd = Context.Guild.GetRole(RoleMap[role.Key]);  // Take the role they want
                        await sender.AddRoleAsync(roleToAdd);  // and give the user the role they want
                        hasRole = true;  // Make sure we respond with the appropriate message

                        await ReplyAsync(BotUtils.KamtroText($"You now have the {role.Key} role!"));  // Reply accordingly

                        break;  // End the loop, no more searching needed.  -C
                    }
                }

                if(!hasRole) {
                    // If the user didn't get a role
                    await ReplyAsync(BotUtils.KamtroText("I can't find a role with that name!"));  // Reply accordingly  -C
                }
            }
        }

        public static Embed GetModifiableRolesInfo() {
            EmbedBuilder builder = new EmbedBuilder();

            return null;
        }
    }
}
