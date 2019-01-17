using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// Stores info on each role.
    /// </summary>
    /// <remarks>
    /// The Name variable is to help you recognize the roles in the config file
    /// </remarks>
    public class RoleInfoNode
    {
        public string Name;
        public string Description;

        public RoleInfoNode(string name, string desc) {
            Name = name;
            Description = desc;
        }
    }
}
