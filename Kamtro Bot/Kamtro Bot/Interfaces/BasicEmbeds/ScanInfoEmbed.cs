using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class ScanInfoEmbed : KamtroEmbedBase
    {
        public int KamexUnique = 0;
        public int RetroUnique = 0;
        public int RetroTotal = 0;
        public int KamexTotal = 0;
        public int Mutual = 0;
        public int Total = 0;

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Ban Scan");
            eb.WithColor(BotUtils.Kamtro);

            eb.AddField("Bans Found (Total | Unique)", $"Kamexico: {KamexTotal}|{KamexUnique}\nRetropolis: {RetroTotal}|{RetroUnique}");
            eb.AddField("Mutual Bans", $"{Mutual}");
            eb.AddField("Total Bans", $"{Total}");

            return eb.Build(); 
        }
    }
}
