using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    public class StrikeDataNode {
        public string Reason;
        public string Moderator;
        public DateTime Date;

        public StrikeDataNode(SocketUser mod, string Reason) {
            Moderator = mod.Username + "#" + mod.Discriminator;
            Date = DateTime.Now;
        }

        public List<string> GetStrikeForExcel() {
            List<string> row = new List<string>();
            row.Add(Date.ToString("F")); // F here means return a string with long date and time format.
            row.Add(Moderator);
            row.Add(Reason);
            return row;
        }
    }
}
