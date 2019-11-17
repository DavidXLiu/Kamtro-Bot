using System;
using System.Numerics;
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
        private const int MAX_SIDES = 2_000_000_000;
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
        public async Task DiceRollAsync([Remainder] string args = "") {
            if(args == "") {
                await ReplyAsync(BotUtils.KamtroText("Please specify parameters as <num>d<sides>"));
                return;
            }

            string[] s = args.ToLower().Split('d');
            int num, sides;

            if(s.Length != 2) {
                await ReplyAsync(BotUtils.KamtroText("Invalid Arguments. Make sure you specify parameters as <num>d<sides>"));
                return;
            }

            if(s[0].Contains(".") || s[1].Contains(".")) {
                await ReplyAsync(BotUtils.KamtroText("Invalid Arguments. Both numbers must be whole numbers."));
                return;
            }

            if (!int.TryParse(s[0], out num) || !int.TryParse(s[1], out sides)) {
                BigInteger bigNum, bigSides;
                // Exceeds maximum int value
                if (!BigInteger.TryParse(s[0], out bigNum) || !BigInteger.TryParse(s[0], out bigSides)) {
                    await ReplyAsync(BotUtils.KamtroText("Something went wrong with your numbers. Please make sure you have no symbols in it except for '-'"));
                } else {
                    if (bigNum > MAX_SIDES || bigSides > MAX_SIDES) {
                        await ReplyAsync(BotUtils.KamtroText("That number is waaaaay too big. Please try a smaller number! (Less than 2 Billion sides, and less than 10 dice)"));
                    } else if(bigNum < -MAX_SIDES || bigSides < -MAX_SIDES) {
                        await ReplyAsync(BotUtils.KamtroText("That number is waaaaay too small. Please try a larger number! (Greater than negative 2 Billion sides and dice)"));
                    }
                }
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

            if(num < 0) {
                await ReplyAsync(BotUtils.KamtroText($"Unrolled {Math.Abs(num)} dice. Didn't get any other numbers out of it though."));
                return;
            }

            if (sides >= -1 && sides <= 1) {
                await ReplyAsync(BotUtils.KamtroText($"Every dice landed on {sides}. What did you expect?"));
                return;
            }

            Random dice = new Random();

            string result = $"Rolled {num} {(num == 1 ? "die" : "dice")}:\n\n";

            for (int i = 0; i < num; i++) {
                result += $"({i + 1}){((i+1 < 10) ? " ":"")} rolled a {dice.Next(Math.Min(1, sides), Math.Max(0, sides))}\n";
            }

            await ReplyAsync(BotUtils.KamtroText(result + "\n"));
        }
    }
}
