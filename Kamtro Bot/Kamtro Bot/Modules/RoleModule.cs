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
        public async Task AddRoleAsync([Remainder]string message) {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);

            // Find if user entered a role
            if (string.IsNullOrWhiteSpace(message)) {
                /// TO DO 
                /// When nothing is entered, make a prompt for selecting a role to add
                /// [DONE] When a role cannot be added, notify the user that it is not a modifiable role
                /// [DONE] When the user already has the role, notify the user that they already have the role
                /// [DONE] Give the role to the user if they do not have the role
                /// [DONE] Notify the user if the role doesn't exist    
                /// 
                /// Current issue: This doesn't work over DM, I'll try to fix it tomorrow

                RoleAdditionEmbed embed = new RoleAdditionEmbed(Context.Message.Author as SocketGuildUser);
                await embed.Display(Context.Channel);
                await embed.AddReactions();

                ulong id = Context.Message.Author.Id;
                if (ReactionHandler.EventQueue.ContainsKey(id)) {
                    // If the user is in the queue
                    ReactionHandler.EventQueue[id].Add(new EventQueueNode(embed));  // Add the action to their list
                } else {
                    // otherwise
                    ReactionHandler.EventQueue.Add(id, new List<EventQueueNode>());  // Create the list
                    ReactionHandler.EventQueue[id].Add(new EventQueueNode(embed));  // And add the action to their list
                }
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
                                break;
                            }
                            // Third person response elsewhere - Arcy
                            else {
                                // Reply using nickname - Arcy
                                if (user.Nickname != null) {
                                    await ReplyAsync(BotUtils.KamtroText($"{user.Nickname} already has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    break;
                                }
                                // Reply using username - Arcy
                                else {
                                    await ReplyAsync(BotUtils.KamtroText($"{user.Username} already has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    break;
                                }
                            }
                        }

                        /// ADDING ROLE
                        // Check if it is a modifiable role - Arcy
                        else if (ServerData.ModifiableRoles.Contains(role)) {
                            // Catch instance that the role is higher in heirarchy than bot role - Arcy
                            if (role.Position >= ServerData.KamtroBotRole.Position) {
                                await ReplyAsync(BotUtils.KamtroText($"Uh oh! I cannot manage that role! Please contact {ServerData.PrimaryContactUser.Username}#{ServerData.PrimaryContactUser.Discriminator} and let them know about this!"));
                                break;
                            } else {
                                // Add the role! Woohoo! - Arcy
                                await user.AddRoleAsync(role);

                                // First person response if DMs - Arcy
                                if (Context.Channel is IDMChannel) {
                                    await ReplyAsync(BotUtils.KamtroText($"You now have the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    break;
                                }
                                // Third person response elsewhere - Arcy
                                else {
                                    // Reply using nickname - Arcy
                                    if (user.Nickname != null) {
                                        await ReplyAsync(BotUtils.KamtroText($"{user.Nickname} now has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                        break;
                                    }
                                    // Reply using username - Arcy
                                    else {
                                        await ReplyAsync(BotUtils.KamtroText($"{user.Username} now has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                        break;
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
                                    break;
                                }
                                // Third person response elsewhere - Arcy
                                else {
                                    // Reply using nickname - Arcy
                                    if (user.Nickname != null) {
                                        await ReplyAsync(BotUtils.KamtroText($"Nice try, {user.Nickname}."));
                                        break;
                                    }
                                    // Reply using username - Arcy
                                    else {
                                        await ReplyAsync(BotUtils.KamtroText($"Nice try, {user.Username}."));
                                        break;
                                    }
                                }
                            }
                            // And more flavor for the bot role - Arcy
                            else if (role == ServerData.KamtroBotRole) {
                                await ReplyAsync(BotUtils.KamtroText($"There can only be one."));
                                break;
                            } else {
                                await ReplyAsync(BotUtils.KamtroText($"The {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role cannot be added."));
                                break;
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
        public async Task RemoveRoleAsync([Remainder]string message) {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);

            // Find if user entered a role
            if (string.IsNullOrWhiteSpace(message)) {
                /// TO DO 
                /// When nothing is entered, make a prompt for selecting a role to remove
                /// [DONE] When a role cannot be removed, notify the user that it is not a modifiable role
                /// [DONE] When the user already doesn't have the role, notify the user that they don't have the role
                /// [DONE] Remove the role to the user if they do not have the role
                /// [DONE] Notify the user if the role doesn't exist
            } else {
                // Check all roles - Arcy
                foreach (SocketRole role in ServerData.AllRoles) {
                    // Find if the message matches the role closely enough - Arcy
                    if (UtilStringComparison.CompareWordScore(message, role.Name) >= 0.66) {
                        /// ALREADY DOESN'T HAVE ROLE
                        // Check if user already doesn't have the role - Arcy
                        if (!user.Roles.Contains(role)) {
                            // First person response if DMs - Arcy
                            if (Context.Channel is IDMChannel) {
                                await ReplyAsync(BotUtils.KamtroText($"You already do not have the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                break;
                            }
                            // Third person response elsewhere - Arcy
                            else {
                                // Reply using nickname - Arcy
                                if (user.Nickname != null) {
                                    await ReplyAsync(BotUtils.KamtroText($"{user.Nickname} already does not have the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    break;
                                }
                                // Reply using username - Arcy
                                else {
                                    await ReplyAsync(BotUtils.KamtroText($"{user.Username} already does not have the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                    break;
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
                                    break;
                                }
                                // Third person response elsewhere - Arcy
                                else {
                                    // Reply using nickname - Arcy
                                    if (user.Nickname != null) {
                                        await ReplyAsync(BotUtils.KamtroText($"{user.Nickname} no longer has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                        break;
                                    }
                                    // Reply using username - Arcy
                                    else {
                                        await ReplyAsync(BotUtils.KamtroText($"{user.Username} no longer has the {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role."));
                                        break;
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
                                    break;
                                }
                                // Third person response elsewhere - Arcy
                                else {
                                    // Reply using nickname - Arcy
                                    if (user.Nickname != null) {
                                        await ReplyAsync(BotUtils.KamtroText($"Just ask Kamex or Retro, {user.Nickname}."));
                                        break;
                                    }
                                    // Reply using username - Arcy
                                    else {
                                        await ReplyAsync(BotUtils.KamtroText($"Just ask Kamex or Retro, {user.Username}."));
                                        break;
                                    }
                                }
                            } else {
                                await ReplyAsync(BotUtils.KamtroText($"The {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.Name.ToLower())} role cannot be removed."));
                                break;
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
            /// TO DO
            /// 
            /// Make an embed that contains all modifiable roles
            /// Show which roles the user currently has on the embed
            /// Display embed
        }
    }
}
