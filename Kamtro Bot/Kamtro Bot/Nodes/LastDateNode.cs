using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kamtro_Bot.Nodes
{
    public class LastDateNode
    {
        public string LastWeeklyReset = "";
        public string LastDailyReset = "";

        public void Save() {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            File.WriteAllText(DataFileNames.LastDateFile, json);
        }
    }
}
