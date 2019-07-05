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

        public StrikeDataNode(SocketUser mod, string reason) {
            Moderator = mod.Username + "#" + mod.Discriminator;
            Date = DateTime.Now;
            Reason = reason;
        }

        public List<string[]> GetStrikeForExcel() {
            List<string[]> row = new List<string[]>();

            row.Add(new string[] { Date.ToString("F"), Moderator, Reason });

            return row;
        }
    }
}
