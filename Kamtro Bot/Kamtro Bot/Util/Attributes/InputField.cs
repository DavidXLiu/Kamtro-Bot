using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Util
{
    /// <summary>
    /// Input field attribute for the forms
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        AllowMultiple = true)]
    public class InputField : Attribute
    {
        // Constructor variables
        public string Name;  // The displayed name of the variable
        public int Page; // The page of the variable
        public int Position;  // The position of the variable on it's page

        // Predefined variables
        public string Value = "[Enter Value]";

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Kamtro_Bot.Util.InputField"/> class.
        /// </summary>
        /// <param name="name">The displayed name of the field</param>
        /// <param name="position">The position of the field on it's page</param>
        /// <param name="page">The page that the field is on</param>
        public InputField(string name, int page, int position) {
            Name = name;
            Page = page;
            Position = position;
        }
    }
}
