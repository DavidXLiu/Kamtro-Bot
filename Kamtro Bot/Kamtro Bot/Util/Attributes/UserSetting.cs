using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Util.Attributes
{
    [AttributeUsage(
            AttributeTargets.Property | AttributeTargets.Field,
            AllowMultiple = true)]
    public class UserSetting : Attribute
    {
        public string Name;
        public string Description;

        public UserSetting(string name, string desc = "") {
            Name = name;
            Description = desc;
        }
    }
}
