using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamtro_Bot.Managers;
using Kamtro_Bot.Nodes;

namespace Kamtro_Bot.Interfaces.ActionEmbeds
{
    public class CoinFlipEmbed : ActionEmbed
    {
        private enum GameState
        {
            WIN,
            LOSS,
            NEUTRAL
        }

        private const string HEADS = "\U0001F1ED";
        private const string TAILS = "\U0001F1F9";

        private SocketGuildUser User;
        private UserDataNode Data;

        private bool LoseFlag = false;
        private bool Result;

        private int Streak = 0;
        private GameState State = GameState.NEUTRAL;

        public CoinFlipEmbed(SocketCommandContext ctx) {
            SetCtx(ctx);

            User = BotUtils.GetGUser(ctx);
            Data = UserDataManager.GetUserData(User);

            AddMenuOptions(new MenuOptionNode(HEADS, "Heads"), new MenuOptionNode(TAILS, "Tails"));
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            if(State == GameState.WIN) {
                eb.WithColor(BotUtils.Green);
            } else if(State == GameState.LOSS) {
                eb.WithColor(BotUtils.Red);
            } else {
                eb.WithColor(BotUtils.Kamtro);
            }

            if(State != GameState.NEUTRAL) {
                eb.AddField("Result", Result ? "Heads" : "Tails");
            }
         
            eb.AddField("Current Streak", Streak, true);
            eb.AddField("Highest Streak", Data.MaxCFStreak, true);
            eb.AddField("% Chance of Getting This Streak", $"{Math.Round(1.0/(Math.Pow(2, Streak + 1)) * 100, 2)}%");

            if(LoseFlag) {
                LoseFlag = false;
                eb.WithDescription("Streak Broken!");
            }

            AddMenu(eb);

            return eb.Build();
        }

        public override async Task PerformAction(SocketReaction option) {
            switch(option.Emote.ToString()) {
                case HEADS:
                    FlipCoin();
                    
                    if(Result) {
                        State = GameState.WIN;
                        Streak++;
                        if(Streak > Data.MaxCFStreak) {
                            Data.MaxCFStreak++;
                            UserDataManager.SaveUserData();
                        }
                    } else {
                        State = GameState.LOSS;
                        Streak = 0;
                        LoseFlag = true;
                    }

                    await UpdateEmbed();
                    break;

                case TAILS:
                    FlipCoin();

                    if (!Result) {
                        State = GameState.WIN;

                        Streak++;
                        if (Streak > Data.MaxCFStreak) {
                            Data.MaxCFStreak++;
                            UserDataManager.SaveUserData();
                        }
                    } else {
                        State = GameState.LOSS;
                        Streak = 0;
                        LoseFlag = true;
                    }

                    await UpdateEmbed();
                    break;
            }
        }

        private void FlipCoin() {
            Random coin = new Random();

            Result = coin.Next(0, 2) == 0;
        }
    }
}
