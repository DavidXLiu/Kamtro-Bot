using System;

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

        public MessageFieldNode(string name, int page, int pos, string value = "[Enter Value]") {
            Name = name;
            Page = page;
            Pos = pos;
            Value = value;
        }

        public void SetValue(string val) {
            Value = val;
        }

        public string GetValue() {
            return Value;
        }
    }
}
