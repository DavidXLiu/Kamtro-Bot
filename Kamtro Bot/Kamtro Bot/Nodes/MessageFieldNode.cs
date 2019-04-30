using System;
using System.Reflection;
using Kamtro_Bot.Interfaces;
using Kamtro_Bot.Util;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// Node for storing info on an input field
    /// </summary>
    public class MessageFieldNode
    {
        public string Name;
        private string Value;
        public int Page;
        public int Pos;

        public MessageEmbed ClassPtr;

        public FieldDataType Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Kamtro_Bot.Nodes.MessageFieldNode"/> class.
        /// </summary>
        /// <param name="name">The displayed name of the field.</param>
        /// <param name="page">The page that the field is on.</param>
        /// <param name="pos">The field's position on it's page.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="dataType">Data type of the field. Default is string.</param>
        public MessageFieldNode(string name, int page, int pos, string value = "[Enter Value]", FieldDataType dataType = FieldDataType.STR) {
            Name = name;
            Page = page;
            Pos = pos;
            Value = value;
            Type = dataType;
        }

        public void SetValue(string val) {
            Value = val;  // set the attribute's value
            
            // and set the actual variables value
            foreach(FieldInfo f in ClassPtr.GetType().GetFields()) {
                if(Attribute.IsDefined(f, typeof(InputField))) {
                    InputField ipf = f.GetCustomAttribute(typeof(InputField)) as InputField;

                    if (ipf.Page == Page && ipf.Position == Pos) {
                        string s = f.Name;

                        ClassPtr.GetType().GetField(s).SetValue(ClassPtr, val);
                        break;
                    }
                }
            }
        }

        public string GetValue() {
            return Value;
        }
    }
}
