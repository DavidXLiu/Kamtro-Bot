using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// This is for the cross-ban system.
    /// </summary>
    public class CrossBanDataNode
    {
        public static readonly string[] Servers = new string[] { "Invalid Index", "Kamexico", "Retropolis" };  // for ID matching. It's more memory-efficient this way, and extendible.

        public int Server;
        public string Moderator;
        public string Reason;

        public CrossBanDataNode(int server, string reason = "") {
            if(server >= Servers.Length) {
                KLog.Warning($"Cross-ban entry for a user had an invalid index! ({server})");
                Server = 0;
            } else {
                Server = server;
            }

            Reason = reason;
        }

        public string GetInfoText() {
            return $"User was banned on {Servers[Server]}" + (string.IsNullOrWhiteSpace(Reason) ? "": $" for **{Reason}**");
        }

        public string GetServer() {
            return Servers[Server];
        }
    }
}
