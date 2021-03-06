﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Interfaces.ActionEmbeds;
using Kamtro_Bot.Interfaces.MessageEmbeds;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Util;
using System;
using System.Net;
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
                // Check if the user is already an admin
                if (Program.Settings.AdminUsers.Contains(users[0].Id))
                {
                    await ReplyAsync(BotUtils.KamtroText(users[0].Username + "#" + users[0].Discriminator + " is already an admin!"));
                    return;
                }

                await AddAdmin(users[0]);
            } else {
                UserSelectionEmbed use = new UserSelectionEmbed(users, AddAdmin, BotUtils.GetGUser(Context));
                await use.Display(Context.Channel);
            }

            Program.ReloadConfig();
        }

        [Command("reload")]
        public async Task ReloadConfigAsync() {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            Program.ReloadConfig();
            ReactionHandler.SetupRoleMap();

            await ReplyAsync(BotUtils.KamtroText("Settings Reloaded."));
        }

        [Command("addmodifiablerole")]
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
            } else if (roles.Count == 1) {
                await AddModifyRole(roles[0]);
            } else {
                RoleSelectionEmbed rse = new RoleSelectionEmbed(roles, AddModifyRole, BotUtils.GetGUser(Context));
                await rse.Display(Context.Channel);
            }
        }
       
        [Command("removemodifiablerole")]
        [Alias("rmfr", "removemfr")]
        public async Task RemModifyRoleAsync([Remainder] string roleName = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            if (string.IsNullOrWhiteSpace(roleName)) {
                await ReplyAsync(BotUtils.KamtroText("Please specify a role name!"));
            }

            ulong id;

            if (ulong.TryParse(roleName, out id) && BotUtils.GetRole(id) != null) {
                await RemModifyRole(BotUtils.GetRole(id));
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
            } else if (roles.Count == 1) {
                await RemModifyRole(roles[0]);
            } else {
                RoleSelectionEmbed rse = new RoleSelectionEmbed(roles, RemModifyRole, BotUtils.GetGUser(Context));
                await rse.Display(Context.Channel);
            }
        }
        
        [Command("permlevel")]
        [Alias("pl")]
        public async Task PermCheckAsync([Remainder] string user = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command
            if (string.IsNullOrWhiteSpace(user)) {
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
                    UserSelectionEmbed use = new UserSelectionEmbed(users, PermCheck, BotUtils.GetGUser(Context));
                    await use.Display(Context.Channel);
                }
            }
        }
        
        [Command("addroleemote")]
        [Alias("are")]
        public async Task AddEmoteRoleAsync([Remainder] string args = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command
            
            if(string.IsNullOrWhiteSpace(args)) {
                await ReplyAsync(BotUtils.KamtroText("Please specify a role!"));
                return;
            }

            ulong id;

            if (ulong.TryParse(args, out id) && BotUtils.GetRole(id) != null) {
                await AddRoleEmote(BotUtils.GetRole(id));
                return;
            }

            // if it's not a recognized ID, treat it as a name

            List<SocketRole> roles = BotUtils.GetRoles(args);

            if (roles.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I couldn't find any roles that matched the name you told me!"));
                return;
            } else if (roles.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("There were too many roles with that name! Please be more specific, or use the role ID"));
                return;
            } else if (roles.Count == 1) {
                await AddRoleEmote(roles[0]);
            } else {
                RoleSelectionEmbed rse = new RoleSelectionEmbed(roles, AddRoleEmote, BotUtils.GetGUser(Context));
                await rse.Display(Context.Channel);
            }
        }
        
        [Command("removeroleemote")]
        [Alias("ree", "remroleemote")]
        public async Task RemEmoteRoleAsync([Remainder] string args = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            if (string.IsNullOrWhiteSpace(args)) {
                await ReplyAsync(BotUtils.KamtroText("Please specify a role!"));
                return;
            }

            ulong id;

            if (ulong.TryParse(args, out id) && BotUtils.GetRole(id) != null) {
                await AddRoleEmote(BotUtils.GetRole(id));
                return;
            }

            // if it's not a recognized ID, treat it as a name

            List<SocketRole> roles = BotUtils.GetRoles(args);

            if (roles.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I couldn't find any roles that matched the name you told me!"));
                return;
            } else if (roles.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("There were too many roles with that name! Please be more specific, or use the role ID"));
                return;
            } else if (roles.Count == 1) {
                await RemRoleEmote(roles[0]);
            } else {
                RoleSelectionEmbed rse = new RoleSelectionEmbed(roles, RemRoleEmote, BotUtils.GetGUser(Context));
                await rse.Display(Context.Channel);
            }
        }

        [Command("replacesettingsfile")]
        [Alias("rsf")]
        public async Task ReplaceSettingsFileAsync([Remainder] string message = "")
        {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            if (Context.Message.Attachments.Count == 1)
            {
                Attachment file = Context.Message.Attachments.ElementAt(0);

                if (file.Filename != "settings.json")
                {
                    await ReplyAsync(BotUtils.KamtroText("The settings file must be a json file named settings.json!"));
                    return;
                }

                // At this point, the file is valid
                string url = file.Url;

                WebClient wc = new WebClient();
                wc.DownloadFile(url, AdminDataManager.StrikeLogPath);

                KLog.Info($"Settings file updated by {BotUtils.GetFullUsername(Context.User)}");
                await ReplyAsync(BotUtils.KamtroText("The settings file has been updated!"));
            }
        }

        [Command("save")]
        public async Task SaveAsync([Remainder] string args = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            if(BotUtils.SaveInProgress) {
                await ReplyAsync(BotUtils.KamtroText("User data is already being saved!"));
            } else {
                BotUtils.PauseSave = true;
                UserDataManager.SaveUserData();
                BotUtils.PauseSave = false;

                await ReplyAsync(BotUtils.KamtroText("User data saved."));
            }
        }

        [Command("settingsfile")]
        [Alias("sf")]
        public async Task SettingsFileAsync([Remainder] string message = "")
        {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            // Check if the file is being used
            await Context.Channel.SendFileAsync("Config/settings.json");
        }

        [Command("setwelcomemessage")]
        [Alias("welcomemessage", "swm")]
        public async Task SetWelcomeMessageAsync([Remainder] string message = "") {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            string msg;

            if(string.IsNullOrWhiteSpace(message)) {
                msg = BotUtils.ZeroSpace;
            } else {
                msg = message;
            }

            Program.Settings.WelcomeMessageTemplate = msg;
            Program.SaveSettings();

            await ReplyAsync(BotUtils.KamtroText($"Welcome message set to '{msg}'"));
        }

        [Command("userdata")]
        [Alias("ud", "userdatafile")]
        public async Task UserDataAsync([Remainder] string message = "")
        {
            if (!ServerData.HasPermissionLevel(BotUtils.GetGUser(Context), ServerData.PermissionLevel.ADMIN)) return;  // This is an admin only command

            // Check if the file is being used
            if (!BotUtils.SaveInProgress)
                await Context.Channel.SendFileAsync("User Data/UserData.json");
            else
                await ReplyAsync(BotUtils.KamtroText("The user data file is currently being saved."));
        }
        #endregion

        #region Moderator

        #endregion

        #region non-commands
        private async Task AddModifyRole(SocketRole role) {
            // Check if already a modifiable role
            if (Program.Settings.ModifiableRoles.Contains(role.Id))
            {
                await ReplyAsync(BotUtils.KamtroText("That role is already a modifiable role."));
                return;
            }
            RoleAdditionEmbed rae = new RoleAdditionEmbed(Context, role);
            await rae.Display();
        }

        private async Task RemModifyRole(SocketRole role) {
            // Check if already not a modifiable role
            if (!Program.Settings.ModifiableRoles.Contains(role.Id))
            {
                await ReplyAsync(BotUtils.KamtroText("That role is not a modifiable role."));
                return;
            }
            RoleAdditionEmbed rae = new RoleAdditionEmbed(Context, role, true);
            await rae.Display();
        }

        private async Task AddAdmin(SocketGuildUser user) {
            // Check if the user is already an admin
            if (Program.Settings.AdminUsers.Contains(user.Id))
            {
                await ReplyAsync(BotUtils.KamtroText(user.Username + "#" + user.Discriminator + " is already an admin!"));
                return;
            }

            AddAdminEmbed admin = new AddAdminEmbed(Context, user);
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
        
        private async Task AddRoleEmote(SocketRole role) {
            RoleEmoteEmbed ree = new RoleEmoteEmbed(Context, role); // REEEEEEEEE
            await ree.Display();
        }

        private async Task RemRoleEmote(SocketRole role) {
            List<string> roleKeys = new List<string>();

            foreach(KeyValuePair<string, ulong> kvp in ReactionHandler.RoleMap) {
                if (kvp.Value == role.Id)
                {
                    // Add the role for removal with each reaction attached to it
                    roleKeys.Add(kvp.Key);
                }
            }

            // Remove the role
            for (int i = 0; i < roleKeys.Count; i++)
            {
                ReactionHandler.RoleMap.Remove(roleKeys[i]);
            }
            ReactionHandler.SaveRoleMap();

            if (roleKeys.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("That role has no emote attached to it!"));
            } else {
                await UpdateRoleMessage();
                await ReplyAsync(BotUtils.KamtroText("Role emote association removed."));
            }
        }

        /// <summary>
        /// Method used to update the bot message that contains the list of modifiable roles.
        /// Users can react to this message to get or remove a role with its corresponding reaction.
        /// Call this method whenever the list of modifiable roles changes.
        /// Author: Arcy
        /// </summary>
        /// <returns></returns>
        private async Task UpdateRoleMessage()
        {
            IMessage roleMessage = await ServerData.Server.GetTextChannel(Program.Settings.RoleSelectChannelID).GetMessageAsync(Program.Settings.RoleSelectMessageID);

            // Form message with each pair in the role map
            string message = BotUtils.ZeroSpace;
            foreach (KeyValuePair<string, ulong> pair in ReactionHandler.RoleMap)
            {
                message += ServerData.Server.GetRole(pair.Value).Mention + " - " + pair.Key + "\n";
            }
            await (roleMessage as RestUserMessage).ModifyAsync(x => x.Content = message);
        }
        #endregion
    }
}
