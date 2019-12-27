using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Handlers;
using Kamtro_Bot.Nodes;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Interfaces.MessageEmbeds
{
    public class TimeZoneSelectEmbed : MessageEmbed
    {
        private const string PATTERN = @"\-[0-9]{1,2}:[0-5][0-9]";

        [InputField("Time Zone Offset", 1, 1)]
        public string TimeZone;

        private bool BadFormat = false;

        public TimeZoneSelectEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);
            RegisterMenuFields();
            AddMenuOptions(ReactionHandler.CHECK);
        }

        public override async Task ButtonAction(SocketReaction action) {
            if(action.Emote.ToString() == ReactionHandler.CHECK_STR) {
                if (Regex.IsMatch(TimeZone, PATTERN)) {
                    TimeZoneNode node = new TimeZoneNode(Regex.Match(TimeZone, PATTERN).Value);
                }
            }
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Time Zone Selection");
            eb.WithColor(BotUtils.Purple);
            eb.WithThumbnailUrl(Context.User.GetAvatarUrl());

            eb.WithDescription("In order to use reminders, and other time-reliant fields I need to know what time zone you are in. Please enter your time zone offset. For mor info on time zone offsets, see https://www.timeanddate.com/time/map/. Note that in some locations, the offset is not a full hour, and can also be off by 15-45 minutes as well. Googling the time zone of your city will help with this.");

            AddEmbedFields(eb);
            AddMenu(eb);

            return eb.Build();
        }
    }
}
