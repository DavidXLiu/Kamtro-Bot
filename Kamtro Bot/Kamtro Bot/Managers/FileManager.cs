using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Managers
{
    class FileManager
    {
        private StreamReader reader;
        private StreamWriter writer;

        public string ReadFullFile(string file) {
            reader = new StreamReader(file);
            return reader.ReadToEnd();
        }
    }
}
