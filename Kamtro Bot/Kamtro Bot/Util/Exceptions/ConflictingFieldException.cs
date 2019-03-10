using System;
namespace Kamtro_Bot.Util.Exceptions
{
    public class ConflictingFieldException : Exception
    {
        public ConflictingFieldException(string a, string b, int page, int pos) : base($"There are some fields that overlap!\nFields '{a}' and '{b}' overlap on page {page} at position {pos}.") {

        }
    }
}
