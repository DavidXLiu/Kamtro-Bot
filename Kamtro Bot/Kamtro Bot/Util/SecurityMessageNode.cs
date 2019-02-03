using System;
using Discord.WebSocket;

namespace Kamtro_Bot.Util
{
    public class SecurityMessageNode
    {
        public DateTime TimeStamp;

        public SecurityMessageNode(SocketUserMessage message) {
            TimeStamp = message.Timestamp.DateTime;

        }
    }
}
