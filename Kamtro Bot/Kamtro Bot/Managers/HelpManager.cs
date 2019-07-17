using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Managers
{
    public class HelpManager
    {
        public static string SelectDir(string path, string dir) {
            return path + "\\" + dir;
        }

        /// <summary>
        /// Gets the file name from the passed in path
        /// </summary>
        /// <param name="dir">The file path</param>
        /// <param name="ext">Whether or not to include the extension. Defaults to false/</param>
        /// <returns>The name of the file</returns>
        public static string GetFileFromPath(string dir, bool ext = false) {
            int sPos = dir.LastIndexOf('/');
            string name = dir.Substring(sPos+1);

            if(!ext) {
                int dot = name.LastIndexOf('.');
                name.Remove(dot, name.Length - dot);
            }

            return name;
        }
    }
}
