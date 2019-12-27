using Discord.Commands;
using Kamtro_Bot.Interfaces.MessageEmbeds;
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
            // TODO: CHECK FOR TIMEZONE
            ReminderEmbed re = new ReminderEmbed(Context);
            await re.Display();
        }
    }
}
