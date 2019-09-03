using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Items
{
    public interface IUsable
    {
        void Use(SocketGuildUser user, params object[] args);
    }
}
