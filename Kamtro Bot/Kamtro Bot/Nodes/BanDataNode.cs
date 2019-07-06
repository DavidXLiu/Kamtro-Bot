using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    public class BanDataNode
    {
        public string Reason;
        public string Moderator;
        public DateTime Date;

        public BanDataNode(SocketUser mod, string reason) {
            Reason = reason;
            Moderator = BotUtils.GetFullUsername(mod);

            Date = DateTime.Now;
        }


        public List<string[]> GetBanForExcel() {
            List<string[]> row = new List<string[]>();

            row.Add(new string[] { Date.ToString("F"), Moderator, Reason });

            return row;
        }
    }
}
