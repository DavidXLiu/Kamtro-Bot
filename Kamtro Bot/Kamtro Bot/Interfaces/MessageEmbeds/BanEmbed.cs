using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Interfaces.BasicEmbeds;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces.MessageEmbeds
{
    public class BanEmbed : MessageEmbed
    {
        [InputField("Reason", 1, 1)]
        public string Reason;

        private const string diamond = "\U0001F4A0";

        private SocketUser Target;
        private bool notifyTarget = true;

        public BanEmbed(SocketCommandContext ctx, SocketUser target) {
            AddMenuOptions(ReactionHandler.CHECK, ReactionHandler.DECLINE, new MenuOptionNode(diamond, "Toggle Notify User"));
            Target = target;
            SetCtx(ctx);
            RegisterMenuFields();
        }

        public override async Task ButtonAction(SocketReaction action) {
            switch(action.Emote.ToString()) {
                case ReactionHandler.CHECK_STR:
                    // Time for security checks

                    // First, the classic null check
                    if(Context.Guild.GetUser(Target.Id) == null) {
                        await Context.Channel.SendMessageAsync(BotUtils.KamtroText("That user does not exist!"));
                        KLog.Info($"User {BotUtils.GetFullUsername(Context.User)} attempted to ban non-existant member {BotUtils.GetFullUsername(Target)}");
                        break;
                    }

                    // next, check to see if Kamtro has perms to ban the user
                    if (!BotUtils.HighestUser(Context.Guild.GetUser(Context.Client.CurrentUser.Id), Context.Guild.GetUser(Context.User.Id))) {
                        await Context.Channel.SendMessageAsync(BotUtils.KamtroText("The user is higher than me, so I cannot ban them."));
                        KLog.Info($"User {BotUtils.GetFullUsername(Context.User)} attempted to ban member {BotUtils.GetFullUsername(Target)} of higher status than bot");
                        break;
                    }

                    // next, check if the caller can ban the user
                    if(!BotUtils.HighestUser(Context.Guild.GetUser(Context.User.Id), Context.Guild.GetUser(Target.Id)) && !ServerData.HasPermissionLevel(Context.Guild.GetUser(Context.User.Id), ServerData.PermissionLevel.ADMIN)) {
                        await Context.Channel.SendMessageAsync(BotUtils.KamtroText("This user is higher than you, and as such you cannot ban them."));
                        KLog.Info($"User {BotUtils.GetFullUsername(Context.User)} attempted to ban member {BotUtils.GetFullUsername(Target)} of higher status than caller");
                        break;
                    }

                    BanDataNode ban = new BanDataNode(Context.User, Reason);

                    if (notifyTarget) {
                        bool sent = await BotUtils.DMUserAsync(Target, new BanNotifyEmbed(ban.Reason).GetEmbed());

                        if (!sent) await Context.Channel.SendMessageAsync(BotUtils.BadDMResponse);
                    }

                    AdminDataManager.AddBan(Target, ban);
                    await Context.Channel.SendMessageAsync(BotUtils.KamtroAngry($"User {BotUtils.GetFullUsername(Target)} has been banned."));
                        
                    await Context.Guild.AddBanAsync(Target.Id);
                    break;

                case diamond:
                    notifyTarget = !notifyTarget;
                    await UpdateEmbed();
                    break;
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle($"Ban {BotUtils.GetFullUsername(Target)}");
            eb.WithThumbnailUrl(Target.GetAvatarUrl());
            eb.WithColor(BotUtils.Red);

            eb.AddField("User Strike Count:", AdminDataManager.GetStrikes(Target).ToString());

            eb.AddField("Will notify user?", notifyTarget ? "Yes" : "No");

            AddEmbedFields(eb);
            AddMenu(eb);

            return eb.Build();
        }
    }
}
