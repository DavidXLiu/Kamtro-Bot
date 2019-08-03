using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

namespace Kamtro_Bot.Modules
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        private const int MAX_SIDES = 10000;
        private const int MAX_DICE = 10;

        [Command("coinflip")]
        [Alias("coin", "cf")]
        public async Task CoinFlipAsync() {
            Random r = new Random();

            if (r.Next() % 2 == 0) {
                await ReplyAsync(BotUtils.KamtroText("Heads."));
            } else {
                await ReplyAsync(BotUtils.KamtroText("Tails."));
            }
        }

        [Command("roll")]
        [Alias("dice", "rolldice")]
        public async Task DiceRollAsync([Remainder] string args) {
            string[] s = args.Split('d');
            int num, sides;

            if (!int.TryParse(s[0], out num) || !int.TryParse(s[1], out sides)) {
                await ReplyAsync(BotUtils.KamtroText("Invalid Arguments. Make sure you specify parameters as <num>d<sides>"));
                return;
            }

            if (num > MAX_DICE) {
                await ReplyAsync(BotUtils.KamtroText("You can't roll more than 10 dice at once!"));
                return;
            }

            if (num == 0) {
                await ReplyAsync(BotUtils.KamtroText("Nothing happened."));
                return;
            }

            if (sides == 0 || sides == 1) {
                await ReplyAsync(BotUtils.KamtroText($"Every dice landed on {sides}. What did you expect?"));
                return;
            }

            Random dice = new Random();

            string result = $"Rolled {num} {(num == 1 ? "die" : "dice")}:\n\n";

            for (int i = 0; i < num; i++) {
                result += $"({i + 1}) rolled a {dice.Next(1, sides)}\n";
            }

            await ReplyAsync(BotUtils.KamtroText(result));
        }

        [Command("roll")]
        [Alias("dice", "rolldice")]
        public async Task DiceRollAsync() {
            await ReplyAsync(BotUtils.KamtroText("Please specify parameters as <num>d<sides>"));
        }
    }
}
