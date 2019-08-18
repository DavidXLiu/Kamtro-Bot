using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

using Kamtro_Bot.Util;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Managers;

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
        public async Task AddRoleAsync([Remainder]string message = "") {
            SocketGuildUser user = BotUtils.GetGUser(Context);

            // Find if user entered a role
            if (string.IsNullOrWhiteSpace(message)) {
                SocketGuildUser _user = BotUtils.GetGUser(Context);
                RoleEmbed embed = new RoleEmbed(Context, _user);

                await embed.Display(Context.Channel);

                ulong id = Context.Message.Author.Id;

                EventQueueManager.AddEvent(embed);

            } else {
                // Check all roles - Arcy
                foreach (SocketRole role in ServerData.AllRoles) {
                    // Find if the message matches the role closely enough - Arcy
                    if (UtilStringComparison.CompareWordScore(message, role.Name) >= 0.66) {
                        /// ALREADY HAS ROLE
                        // Check if user already has the role - Arcy
                        if (user.Roles.Contains(role)) {
                            // First person response if DMs - Arcy
                            if (Context.Channel is IDMChannel) {
                                await ReplyAsync(BotUtils.KamtroText($"You already have the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                return;
                            }
                            // Third person response elsewhere - Arcy
                            else {
                                // Reply using nickname - Arcy
                                if (user.Nickname != null) {
                                    await ReplyAsync(BotUtils.KamtroText($"{user.Nickname} already has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    return;
                                }
                                // Reply using username - Arcy
                                else {
                                    await ReplyAsync(BotUtils.KamtroText($"{user.Username} already has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    return;
                                }
                            }
                        }

                        /// ADDING ROLE
                        // Check if it is a modifiable role - Arcy
                        else if (ServerData.ModifiableRoles.Contains(role)) {
                            // Catch instance that the role is higher in heirarchy than bot role - Arcy
                            if (role.Position >= ServerData.KamtroBotRole.Position) {
                                await ReplyAsync(BotUtils.KamtroText($"Uh oh! I cannot manage that role! Please contact {ServerData.PrimaryContactUser.Username}#{ServerData.PrimaryContactUser.Discriminator} and let them know about this!"));
                                return;
                            } else {
                                // Add the role! Woohoo! - Arcy
                                await user.AddRoleAsync(role);

                                // First person response if DMs - Arcy
                                if (Context.Channel is IDMChannel) {
                                    await ReplyAsync(BotUtils.KamtroText($"You now have the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    return;
                                }
                                // Third person response elsewhere - Arcy
                                else {
                                    // Reply using nickname - Arcy
                                    if (user.Nickname != null) {
                                        await ReplyAsync(BotUtils.KamtroText($"{user.Nickname} now has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                        return;
                                    }
                                    // Reply using username - Arcy
                                    else {
                                        await ReplyAsync(BotUtils.KamtroText($"{user.Username} now has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                        return;
                                    }
                                }
                            }
                        }

                        /// NOT MODIFIABLE
                        // Not modifiable. Respond with the role not being able to be added/removed - Arcy
                        else {
                            // Extra flavor for adding mod/admin roles - Arcy
                            if (ServerData.ModeratorRoles.Contains(role) || role.Permissions.Administrator) {
                                // First person response if DMs - Arcy
                                if (Context.Channel is IDMChannel) {
                                    await ReplyAsync(BotUtils.KamtroText($"Nice try."));
                                    return;
                                }
                                // Third person response elsewhere - Arcy
                                else {
                                    // Reply using nickname - Arcy
                                    if (user.Nickname != null) {
                                        await ReplyAsync(BotUtils.KamtroText($"Nice try, {user.Nickname}."));
                                        return;
                                    }
                                    // Reply using username - Arcy
                                    else {
                                        await ReplyAsync(BotUtils.KamtroText($"Nice try, {user.Username}."));
                                        return;
                                    }
                                }
                            }
                            // And more flavor for the bot role - Arcy
                            else if (role == ServerData.KamtroBotRole) {
                                await ReplyAsync(BotUtils.KamtroText($"There can only be one."));
                                return;
                            } else {
                                await ReplyAsync(BotUtils.KamtroText($"The {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role cannot be added."));
                                return;
                            }
                        }
                    }
                }

                /// ROLE CANNOT BE FOUND
                await ReplyAsync(BotUtils.KamtroText($"I do not recognize that role. Please make sure you are spelling the role correctly. (Copy-Paste should always work!)"));
            }
        }

        [Command("removerole"), Alias("remove", "de", "de-", "give me")]
        [Name("RemoveRole")]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        [Summary("Removes an allowed role to the user, unless they already don't have it or are restricted from removing it.")]
        public async Task RemoveRoleAsync([Remainder]string message = "") {
            SocketGuildUser user = BotUtils.GetGUser(Context);

            // Find if user entered a role
            if (string.IsNullOrWhiteSpace(message)) {
                SocketGuildUser _user = BotUtils.GetGUser(Context);
                RoleEmbed embed = new RoleEmbed(Context, _user);

                await embed.Display(Context.Channel);

                ulong id = Context.Message.Author.Id;
                if (EventQueueManager.EventQueue.ContainsKey(id)) {
                    // If the user is in the queue
                    EventQueueManager.EventQueue[id].Add(new EventQueueNode(embed));  // Add the action to their list
                } else {
                    // otherwise
                    EventQueueManager.EventQueue.Add(id, new List<EventQueueNode>());  // Create the list
                    EventQueueManager.EventQueue[id].Add(new EventQueueNode(embed));  // And add the action to their list
                }
            } else {
                // Check all roles - Arcy
                foreach (SocketRole role in ServerData.AllRoles) {
                    // Find if the message matches the role closely enough - Arcy
                    if (UtilStringComparison.CompareWordScore(message, role.Name) >= 0.66) {
                        // ALREADY DOESN'T HAVE ROLE
                        // Check if user already doesn't have the role - Arcy
                        if (!user.Roles.Contains(role)) {
                            // First person response if DMs - Arcy
                            if (Context.Channel is IDMChannel) {
                                await ReplyAsync(BotUtils.KamtroText($"You already do not have the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                return;
                            }
                            // Third person response elsewhere - Arcy
                            else {
                                // Reply using nickname - Arcy
                                if (user.Nickname != null) {
                                    await ReplyAsync(BotUtils.KamtroText($"{user.Nickname} already does not have the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    return;
                                }
                                // Reply using username - Arcy
                                else {
                                    await ReplyAsync(BotUtils.KamtroText($"{user.Username} already does not have the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    return;
                                }
                            }
                        }

                        /// REMOVING ROLE
                        // Check if it is a modifiable role - Arcy
                        else if (ServerData.ModifiableRoles.Contains(role)) {
                            // Catch instance that the role is higher in heirarchy than bot role - Arcy
                            if (role.Position >= ServerData.KamtroBotRole.Position) {
                                await ReplyAsync(BotUtils.KamtroText($"Uh oh! I cannot manage that role! Please contact {ServerData.PrimaryContactUser.Mention} ({ServerData.PrimaryContactUser.Username}#{ServerData.PrimaryContactUser.Discriminator}) and let them know about this!"));
                                break;
                            } else {
                                // Remove the role! Woohoo! - Arcy
                                await user.RemoveRoleAsync(role);

                                // First person response if DMs - Arcy
                                if (Context.Channel is IDMChannel) {
                                    await ReplyAsync(BotUtils.KamtroText($"You no longer have the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    return;
                                }
                                // Third person response elsewhere - Arcy
                                else {
                                    // Reply using nickname - Arcy
                                    if (user.Nickname != null) {
                                        await ReplyAsync(BotUtils.KamtroText($"{user.Nickname} no longer has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                        return;
                                    }
                                    // Reply using username - Arcy
                                    else {
                                        await ReplyAsync(BotUtils.KamtroText($"{user.Username} no longer has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                        return;
                                    }
                                }
                            }
                        }

                        /// NOT MODIFIABLE
                        // Not modifiable. Respond with the role not being able to be added/removed - Arcy
                        else {
                            // Extra flavor for removing mod roles - Arcy
                            if (ServerData.ModeratorRoles.Contains(role)) {
                                // First person response if DMs - Arcy
                                if (Context.Channel is IDMChannel) {
                                    await ReplyAsync(BotUtils.KamtroText($"Just ask Kamex or Retro."));
                                    return;
                                }
                                // Third person response elsewhere - Arcy
                                else {
                                    // Reply using nickname - Arcy
                                    if (user.Nickname != null) {
                                        await ReplyAsync(BotUtils.KamtroText($"Just ask Kamex or Retro, {user.Nickname}."));
                                        return;
                                    }
                                    // Reply using username - Arcy
                                    else {
                                        await ReplyAsync(BotUtils.KamtroText($"Just ask Kamex or Retro, {user.Username}."));
                                        return;
                                    }
                                }
                            } else {
                                await ReplyAsync(BotUtils.KamtroText($"The {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role cannot be removed."));
                                return;
                            }
                        }
                    }
                }

                /// ROLE CANNOT BE FOUND
                await ReplyAsync(BotUtils.KamtroText($"I do not recognize that role. Please make sure you are spelling the role correctly. (Copy-Paste should always work!)"));
            }
        }

        [Command("roles"), Alias("role")]
        [Name("Roles")]
        [Summary("Displays an embed showing the modifiable roles that can be added/removed by users.")]
        public async Task RolesAsync() {
            SocketGuildUser _user = BotUtils.GetGUser(Context);
            RoleEmbed embed = new RoleEmbed(Context, _user);

            await embed.Display(Context.Channel);
        }
    }
}
