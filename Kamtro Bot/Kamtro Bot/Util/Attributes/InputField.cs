using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Util
{
    [AttributeUsage(
        AttributeTargets.Field,
        AllowMultiple = true)]
    public class InputField : Attribute
    {
        public string Name;
        public string Value;

        public InputField(string name, string value = "Enter Value", bool brackets = true) {
            Name = name;

            if(brackets) {
                Value = $"[{value}]";  
            } else {
                Value = value;
            }    
        }
    }
}
