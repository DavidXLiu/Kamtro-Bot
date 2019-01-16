using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot
{
    public class FileManager
    {
        public static string ReadFullFile(string file) {
            StreamReader reader = new StreamReader(file);
            string s = reader.ReadToEnd();
            reader.Close();
            return s;
        }
    }
}
