using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Modules
{
    /// <summary>
    /// Help Command module, for commands related to help
    /// </summary>
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Alias("h", "helpme", "commands")]
        public async Task HelpCommandAsync() {

        }
    }
}
