using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    public class UserSettingsNode
    {
        public string Username;
        public bool TitleNotify;
        public bool AllNotify;

        public UserSettingsNode(string username) {
            Username = username;

            TitleNotify = true;
            AllNotify = true;
        }
    }
}
