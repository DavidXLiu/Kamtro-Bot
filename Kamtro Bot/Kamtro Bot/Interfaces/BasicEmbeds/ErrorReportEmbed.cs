using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Kamtro_Bot.Interfaces.BasicEmbeds
{
    public class ErrorReportEmbed : KamtroEmbedBase
    {
        private Exception Error;

        public ErrorReportEmbed(Exception e) {
            Error = e;
        }

        public override Embed GetEmbed() {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("An Error Occured!");
            eb.WithColor(BotUtils.Orange);

            string st = Error.StackTrace;
            st = st.Substring(st.IndexOf("---")).Trim('\n', '\r', ' ');

            eb.AddField("Type", Error.GetType().ToString());
            eb.AddField("Source", Error.Source);
            eb.AddField("Method", Error.TargetSite);
            eb.AddField("Stack Trace", st);
            eb.AddField("Description", Error.Message);

            eb.WithDescription("Something went wrong with your command! Please ping Arcy or Carbon, or DM them a screenshot of this embed, and make sure you specify what command you used, and how you used it!");

            return eb.Build();
        }
    }
}
