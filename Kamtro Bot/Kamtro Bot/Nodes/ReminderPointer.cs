using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    public class ReminderPointer
    {
        public ulong User;
        public string Date;
        public int Index;

        public ReminderPointer(ulong user, string date, int index) {
            User = user;
            Date = date;
            Index = index;
        }
    }
}
