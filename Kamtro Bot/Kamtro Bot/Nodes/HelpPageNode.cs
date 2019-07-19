using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    public class HelpPageNode
    {
        public string Name;
        public string[] Alias;
        public string Usage;
        public string Description;
        public string GifURL;

        public HelpPageNode(string name, string[] alias, string usage, string desc, string gifURL = "") {
            Name = name;
            Usage = usage;
            Alias = alias;
            Description = desc;
            GifURL = gifURL;
        }
    }
}
