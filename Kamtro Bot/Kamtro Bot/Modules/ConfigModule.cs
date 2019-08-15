using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Interfaces.ActionEmbeds;
using Kamtro_Bot.Interfaces.MessageEmbeds;
using Kamtro_Bot.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Modules
{
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        #region Admin
        [Command("admin")]
        [Alias("addadmin", "aa")]
        public async Task AddAdminAsync([Remainder] string user = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            // This command adds an admin to the bot config.

            List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

            if (users.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I can't find a user with that name, make sure the name is spelt correctly!"));
                return;
            } else if (users.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("Please be more specific! You can attach a discriminator if you need to (Username#1234)"));
                return;
            } else if (users.Count == 1) {
                await AddAdmin(users[0]);
            } else {
                UserSelectionEmbed use = new UserSelectionEmbed(users, AddAdmin, Context.Guild.GetUser(Context.User.Id));
                await use.Display(Context.Channel);
            }
        }

        [Command("reload")]
        public async Task ReloadConfigAsync() {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            Program.LoadSettings();
            ServerData.SetupServerData(Program.Settings);

            await ReplyAsync(BotUtils.KamtroText("Serttings Reloaded."));
        }

        [Command("addmodifyablerole")]
        [Alias("amfr", "addmfr")]
        public async Task AddModifyRoleAsync([Remainder] string roleName = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command
            
            if(string.IsNullOrWhiteSpace(roleName)) {
                await ReplyAsync(BotUtils.KamtroText("Please specify a role name!"));
            }

            ulong id;

            if(ulong.TryParse(roleName, out id) && BotUtils.GetRole(id) != null) {
                await AddModifyRole(BotUtils.GetRole(id));
                return;
            }

            // if it's not a recognized ID, treat it as a name

            List<SocketRole> roles = BotUtils.GetRoles(roleName);

            if(roles.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I couldn't find any roles that matched the name you told me!"));
                return;
            } else if(roles.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("There were too many roles with that name! Please be more specific, or use the role ID"));
                return;
            } else {
                RoleSelectionEmbed rse = new RoleSelectionEmbed(roles, AddModifyRole, BotUtils.GetGUser(Context));
                await rse.Display(Context.Channel);
            }
        }
       
        [Command("removemodifyablerole")]
        [Alias("rmfr", "removemfr")]
        public async Task RemModifyRoleAsync([Remainder] string roleName = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            if (string.IsNullOrWhiteSpace(roleName)) {
                await ReplyAsync(BotUtils.KamtroText("Please specify a role name!"));
            }

            ulong id;

            if (ulong.TryParse(roleName, out id) && BotUtils.GetRole(id) != null) {
                await AddModifyRole(BotUtils.GetRole(id));
                return;
            }

            // if it's not a recognized ID, treat it as a name

            List<SocketRole> roles = new List<SocketRole>();

            foreach(SocketRole r in BotUtils.GetRoles(roleName)) {
                if(Program.Settings.ModifiableRoles.Contains(r.Id)) {
                    roles.Add(r);
                }
            }

            if (roles.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I couldn't find any roles on the list that matched the name you told me!"));
                return;
            } else if (roles.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("There were too many roles with that name! Please be more specific, or use the role ID"));
                return;
            } else {
                RoleSelectionEmbed rse = new RoleSelectionEmbed(roles, RemModifyRole, BotUtils.GetGUser(Context));
                await rse.Display(Context.Channel);
            }
        }
        #endregion

        #region Moderator

        #endregion

        #region non-commands
        private async Task AddModifyRole(SocketRole role) {
            RoleAdditionEmbed rae = new RoleAdditionEmbed(Context, role);
            await rae.Display();
        }

        private async Task RemModifyRole(SocketRole role) {
            RoleAdditionEmbed rae = new RoleAdditionEmbed(Context, role, true);
            await rae.Display();
        }

        private async Task AddAdmin(SocketGuildUser user) {
            AddAdminEmbed admin = new AddAdminEmbed(user);
            await admin.Display();
        }
        #endregion
    }
}
