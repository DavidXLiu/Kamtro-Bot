﻿using Discord.Commands;
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

            Program.ReloadConfig();
        }

        [Command("reload")]
        public async Task ReloadConfigAsync() {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            Program.ReloadConfig();
            await ReplyAsync(BotUtils.KamtroText("Settings Reloaded."));
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
        
        [Command("permlevel")]
        [Alias("pl")]
        public async Task PermCheckAsync([Remainder] string user = "") {
            if(string.IsNullOrWhiteSpace(user)) {
                await PermCheck(BotUtils.GetGUser(Context));
            } else {
                List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

                if (users.Count == 0) {
                    await ReplyAsync(BotUtils.KamtroText("I can't find a user with that name, make sure the name is spelt correctly!"));
                    return;
                } else if (users.Count > 10) {
                    await ReplyAsync(BotUtils.KamtroText("Please be more specific! You can attach a discriminator if you need to (Username#1234)"));
                    return;
                } else if (users.Count == 1) {
                    await PermCheck(users[0]);
                } else {
                    UserSelectionEmbed use = new UserSelectionEmbed(users, PermCheck, Context.Guild.GetUser(Context.User.Id));
                    await use.Display(Context.Channel);
                }
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
            await admin.Display(Context.Channel);
        }
        
        private async Task PermCheck(SocketGuildUser user) {
            int perm = 0;

            if (ServerData.HasPermissionLevel(user, ServerData.PermissionLevel.USER)) {
                perm++;

                if (ServerData.HasPermissionLevel(user, ServerData.PermissionLevel.TRUSTED)) {
                    perm++;

                    if (ServerData.HasPermissionLevel(user, ServerData.PermissionLevel.MODERATOR)) {
                        perm++;

                        if (ServerData.HasPermissionLevel(user, ServerData.PermissionLevel.ADMIN)) {
                            perm++;
                        }
                    }
                }
            }

            BasicEmbed be;

            switch (perm) {
                case 1:
                    be = new BasicEmbed($"Permission Level For User {BotUtils.GetFullUsername(user)}", "User", "Permission Level:", BotUtils.Kamtro, user.GetAvatarUrl());
                    break;

                case 2:
                    be = new BasicEmbed($"Permission Level For User {BotUtils.GetFullUsername(user)}", "Trusted", "Permission Level:", BotUtils.Green, user.GetAvatarUrl());
                    break;

                case 3:
                    be = new BasicEmbed($"Permission Level For User {BotUtils.GetFullUsername(user)}", "Moderator", "Permission Level:", BotUtils.Blue, user.GetAvatarUrl());
                    break;

                case 4:
                    be = new BasicEmbed($"Permission Level For User {BotUtils.GetFullUsername(user)}", "Admin", "Permission Level:", BotUtils.Purple, user.GetAvatarUrl());
                    break;

                default:
                    be = new BasicEmbed($"Permission Level For User {BotUtils.GetFullUsername(user)}", "Muted", "Permission Level:", BotUtils.Grey, user.GetAvatarUrl());
                    break;
            }

            await be.Display(Context.Channel);
        }
        #endregion
    }
}
