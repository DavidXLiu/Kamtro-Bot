using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;

using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Util;
using Kamtro_Bot.Interfaces.MessageEmbeds;
using Kamtro_Bot.Managers;
using System.IO;
using System.Net;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// This is the moderator module. Users marked as admins or have the moderator roles may use these commands.
    /// </summary>
    [Name("Moderator")]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        [Command("userinfo")]
        [Alias("info","whois","identify")]
        public async Task UserInfoAsync([Remainder] string args = null)
        {
            // Moderator check
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            if (ServerData.HasPermissionLevel(user, ServerData.PermissionLevel.MODERATOR) || user.Id == 118892308086128641)
            {
                // Only command was used. Get info on current user
                if (string.IsNullOrWhiteSpace(args))
                {
                    await DisplayUserInfoEmbed(user);
                    return;
                }
                else
                {
                    List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

                    // No user found
                    if (users.Count == 0)
                    {
                        await ReplyAsync(BotUtils.KamtroText("I could not find that user. Please try again and make sure you are spelling the user correctly."));
                        return;
                    }
                    // Get info on user
                    else if (users.Count == 1)
                    {
                        await DisplayUserInfoEmbed(users[0]);
                        return;
                    }
                    // Name is too vague. More than 10 users found
                    else if (users.Count > 10)
                    {
                        await ReplyAsync(BotUtils.KamtroText("That name is too vague. Please try specifying the user."));
                        return;
                    }
                    // More than one user mentioned, or ambiguous user
                    else
                    {
                        UserSelectionEmbed use = new UserSelectionEmbed(users, DisplayUserInfoEmbed, ServerData.Server.GetUser(Context.User.Id));
                        await use.Display(Context.Channel);
                    }
                }
            }
        }

        [Command("voicekick")]
        [Alias("vckick","kickvc","kickvoice","removevc","removevoicechat")]
        public async Task VoiceKickAsync([Remainder] string args = null)
        {
            // Moderator check
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            if (ServerData.HasPermissionLevel(user, ServerData.PermissionLevel.MODERATOR) || user.Id == 118892308086128641)
            {
                // Only command was used. Reply to user saying a user needs to be specified for the command.
                if (String.IsNullOrWhiteSpace(args))
                {
                    await ReplyAsync(BotUtils.KamtroText("You did not specify the user to remove from voice chat."));
                    return;
                }
                else
                {
                    List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

                    // No user found
                    if (users.Count == 0)
                    {
                        await ReplyAsync(BotUtils.KamtroText("I could not find that user. Please try again and make sure you are spelling the user correctly."));
                        return;
                    }
                    // Check for users in voice channels and remove those that aren't
                    else
                    {
                        List<SocketGuildUser> removeUsers = new List<SocketGuildUser>();
                        foreach (SocketGuildUser currentUser in users)
                        {
                            if (currentUser.VoiceChannel == null)
                            {
                                removeUsers.Add(currentUser);
                            }
                        }

                        foreach (SocketGuildUser currentUser in removeUsers)
                        {
                            users.Remove(currentUser);
                        }
                    }

                    // No users found in voice channels
                    if (users.Count == 0)
                    {
                        await ReplyAsync(BotUtils.KamtroText("That user is not in a voice channel."));
                    }
                    // Create temporary voice channel, move user, then delete voice channel
                    else if (users.Count == 1)
                    {
                        SocketVoiceChannel currentVc = users[0].VoiceChannel;
                        RestVoiceChannel vcChannel = await ServerData.Server.CreateVoiceChannelAsync("Temporary");
                        await users[0].ModifyAsync(x => x.Channel = vcChannel);
                        await vcChannel.DeleteAsync();

                        if (users[0].Nickname != null)
                        {
                            await ReplyAsync(BotUtils.KamtroText($"{users[0].Nickname} has been removed from {currentVc.Name}."));
                        }
                        else
                        {
                            await ReplyAsync(BotUtils.KamtroText($"{users[0].Username} has been removed from {currentVc.Name}."));
                        }

                        return;
                    }
                    // Name is too vague. More than 10 users found
                    else if (users.Count > 10)
                    {
                        await ReplyAsync(BotUtils.KamtroText("That name is too vague. Please try specifying the user."));
                        return;
                    }
                    // More than one user mentioned, or ambiguous user
                    else
                    {
                        UserSelectionEmbed use = new UserSelectionEmbed(users, VoiceKickUserAsync, ServerData.Server.GetUser(Context.User.Id));
                        await use.Display(Context.Channel);
                    }
                }
            }
        }

        [Command("strike")]
        [Alias("addstrike", "adds")]
        public async Task AddStrikeAsync([Remainder] string name = "") {
            SocketGuildUser caller = Context.Guild.GetUser(Context.User.Id);
            if (!ServerData.HasPermissionLevel(caller, ServerData.PermissionLevel.MODERATOR)) return;

            if (name == "") {
                await ReplyAsync(BotUtils.KamtroText("Please specify a user!"));
                return;
            }

            List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

            if(users.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I can't find a user with that name, make sure the name is spelt correctly!"));
                return;
            } else if(users.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("Please be more specific! You can attach a discriminator if you need to (Username#1234)"));
                return;
            } else if(users.Count == 1) {
                await StrikeUser(users[0]);
            } else {
                UserSelectionEmbed use = new UserSelectionEmbed(users, StrikeUser, Context.Guild.GetUser(Context.User.Id));
                await use.Display(Context.Channel);
            }
        }

        [Command("strikelog")]
        [Alias("getstrikelog", "sl")]
        public async Task StrikeLogAsync() {
            if (!ServerData.HasPermissionLevel(Context.Guild.GetUser(Context.User.Id), ServerData.PermissionLevel.MODERATOR)) return;

            if (File.Exists(AdminDataManager.StrikeLogPath)) {
                await Context.Channel.SendFileAsync(AdminDataManager.StrikeLogPath);
            } else {
                AdminDataManager.InitExcel();
                await ReplyAsync(BotUtils.KamtroText("Stirke file was missing, so I made a new one."));
                await Context.Channel.SendFileAsync(AdminDataManager.StrikeLogPath);
            }
        }

        [Command("editstrikelog")]
        [Alias("esl", "replacestrikelog")]
        public async Task EditStrikeLogAsync() {
            if (!ServerData.HasPermissionLevel(Context.Guild.GetUser(Context.User.Id), ServerData.PermissionLevel.MODERATOR)) return;

            if(Context.Message.Attachments.Count != 1) {
                await ReplyAsync(BotUtils.KamtroText("Please attach the strike log file with no other attachments!"));
                return;
            }

            Attachment file = Context.Message.Attachments.ElementAt(0);

            if(file.Filename != "strikelog.xlsx") {
                await ReplyAsync(BotUtils.KamtroText("The strike log must be an excel file named strikelog.xlsx!"));
                return;
            }

            // At this point, the file is valid
            string url = file.Url;

            WebClient wc = new WebClient();
            wc.DownloadFile(url, AdminDataManager.StrikeLogPath);

            KLog.Info($"Strike log updated by {BotUtils.GetFullUsername(Context.User)}");
            await ReplyAsync(BotUtils.KamtroText("The strike log has been updated!"));
        }

        [Command("ban")]
        [Alias("banuser")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanMemberAsync([Remainder] string name = "") {
            SocketGuildUser caller = Context.Guild.GetUser(Context.User.Id);
            if (!ServerData.HasPermissionLevel(caller, ServerData.PermissionLevel.MODERATOR)) return;

            if (name == "") {
                await ReplyAsync(BotUtils.KamtroText("Please specify a user!"));
                return;
            }

            List<SocketGuildUser> users = BotUtils.GetUser(Context.Message);

            if (users.Count == 0) {
                await ReplyAsync(BotUtils.KamtroText("I can't find a user with that name, make sure the name is spelt correctly!"));
                return;
            } else if (users.Count > 10) {
                await ReplyAsync(BotUtils.KamtroText("Please be more specific! You can attach a discriminator if you need to (Username#1234)"));
                return;
            } else if (users.Count == 1) {
                await BanUser(users[0]);
            } else {
                UserSelectionEmbed use = new UserSelectionEmbed(users, BanUser, Context.Guild.GetUser(Context.User.Id));
                await use.Display(Context.Channel);
            }
        }

        #region Helper Methods
        private async Task StrikeUser(SocketUser user) {
            if(AdminDataManager.GetStrikes(user) >= 2) {
                await BanUser(user);
                return;
            }

            StrikeEmbed se = new StrikeEmbed(Context, user);
            await se.Display();
        }

        private async Task BanUser(SocketUser user) {
            // First, the classic null check
            if (Context.Guild.GetUser(user.Id) == null) {
                await Context.Channel.SendMessageAsync(BotUtils.KamtroText("That user does not exist!"));
                KLog.Info($"User {BotUtils.GetFullUsername(Context.User)} attempted to ban non-existant member {BotUtils.GetFullUsername(user)}");
                return;
            }

            // next, check to see if Kamtro has perms to ban the user
            if (!BotUtils.HighestUser(Context.Guild.GetUser(Context.Client.CurrentUser.Id), Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(BotUtils.KamtroText("The user is higher than me, so I cannot ban them."));
                KLog.Info($"User {BotUtils.GetFullUsername(Context.User)} attempted to ban member {BotUtils.GetFullUsername(user)} of higher status than bot");
                return;
            }

            // next, check if the caller can ban the user
            if (!BotUtils.HighestUser(Context.Guild.GetUser(Context.User.Id), Context.Guild.GetUser(user.Id)) && !ServerData.HasPermissionLevel(Context.Guild.GetUser(Context.User.Id), ServerData.PermissionLevel.ADMIN)) {
                await Context.Channel.SendMessageAsync(BotUtils.KamtroText("This user is higher than you, and as such you cannot ban them."));
                KLog.Info($"User {BotUtils.GetFullUsername(Context.User)} attempted to ban member {BotUtils.GetFullUsername(user)} of higher status than caller");
                return;
            }

            BanEmbed be = new BanEmbed(Context, user);
            await be.Display();
        }

        private async Task DisplayUserInfoEmbed(SocketGuildUser user)
        {
            EmbedBuilder embed = new EmbedBuilder();

            string embedText = "";
            embedText += $"User: {user.Mention}\n";
            embedText += $"Account Created: {user.CreatedAt.Month}/{user.CreatedAt.Day}/{user.CreatedAt.Year}\n";
            embedText += $"Joined Server: {user.JoinedAt.Value.Month}/{user.JoinedAt.Value.Day}/{user.JoinedAt.Value.Year}\n";
            embedText += $"Avatar URL: {user.GetAvatarUrl()}";

            Color color = new Color(0, 0, 0);
            foreach (SocketRole role in user.Roles)
            {
                if (role.Color.RawValue != 0)
                {
                    color = role.Color;
                    break;
                }
            }

            embed.WithAuthor($"{user.Username}#{user.Discriminator}", user.GetAvatarUrl());
            embed.AddField("User Info", embedText);
            embed.WithColor(color);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        private async Task VoiceKickUserAsync(SocketGuildUser user)
        {
            SocketVoiceChannel currentVc = user.VoiceChannel;
            RestVoiceChannel vcChannel = await ServerData.Server.CreateVoiceChannelAsync("Temporary");
            await user.ModifyAsync(x => x.Channel = vcChannel);
            await vcChannel.DeleteAsync();

            if (user.Nickname != null)
            {
                await ReplyAsync(BotUtils.KamtroText($"{user.Nickname} has been removed from {currentVc.Name}."));
            }
            else
            {
                await ReplyAsync(BotUtils.KamtroText($"{user.Username} has been removed from {currentVc.Name}."));
            }
        }
        #endregion
    }
}
