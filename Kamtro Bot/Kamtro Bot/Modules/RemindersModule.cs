using Discord.Commands;
using Kamtro_Bot.Interfaces.MessageEmbeds;
using Kamtro_Bot.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Modules
{
    public class RemindersModule : ModuleBase<SocketCommandContext>
    {
        [Command("reminder")]
        [Alias("reminders", "notifications", "rmnd")]
        public async Task ReminderCommandAsync([Remainder] string args = "") {
            // TODO: CHECK FOR USER TIMEZONE
            if(UserDataManager.GetUserData(BotUtils.GetGUser(Context)).TimeZone == null) {
                await ReplyAsync(BotUtils.KamtroText("Looks like you don't have a time zone set! Set one now and do the command again to set reminders."));
                TimeZoneSelectEmbed tzse = new TimeZoneSelectEmbed(Context);
                await tzse.Display();
                return;
            }

            ReminderEmbed re = new ReminderEmbed(Context);
            await re.Display();
        }
    }
}
