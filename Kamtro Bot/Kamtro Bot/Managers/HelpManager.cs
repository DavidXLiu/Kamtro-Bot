using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kamtro_Bot.Nodes;
using Newtonsoft.Json;

namespace Kamtro_Bot.Managers
{
    public class HelpManager
    {
        public const string EmptyDirMsg = "Nothing here.";

        public static string[] GetDirList(string dir, bool admin, bool mod) {
           List<string> dirs = new List<string>();

            dirs.AddRange(Directory.GetDirectories(dir));
            dirs.AddRange(Directory.GetFiles(dir, "*.json"));
            dirs.AddRange(Directory.GetFiles(dir, "*.txt"));

            dirs.Sort();

            if (!admin) {
                List<string> toRem = new List<string>();

                foreach (string d in dirs) {
                    if (d.ToLower().Contains("admin")) {
                        toRem.Add(d);
                    }

                    if(!mod && d.ToLower().Contains("moderator") && !toRem.Contains(d)) {
                        toRem.Add(d);
                    }
                }

                foreach(string s in toRem) {
                    dirs.Remove(s);
                }
            }

            return dirs.ToArray();
        }
        
        /// <summary>
        /// Returns the gelp file as a node object
        /// </summary>
        /// <param name="path">Path to the help file</param>
        /// <returns>The node corresponding to the file</returns>
        public static HelpPageNode GetNode(string path) {
            string json = FileManager.ReadFullFile(path);

            HelpPageNode help = JsonConvert.DeserializeObject<HelpPageNode>(json);

            return help;
        }

        #region Helper Methods
        public static string StripExtraDirs(string dir) {
            dir.TrimEnd('\\');
            dir = dir.Substring(dir.LastIndexOf('\\') + 1);
            return dir;
        }

        /// <summary>
        /// Adds the dir to your path
        /// </summary>
        /// <param name="path">Current file path</param>
        /// <param name="dir">Chosen Directory</param>
        /// <returns></returns>
        public static string SelectDir(string path, string dir) {
            return path + "\\" +  StripExtraDirs(dir);
        }

        public static string BackDir(string path) {
            if(!path.Contains('\\')) {
                return "Help"; // Don't go before help dir
            }

            int pos = path.LastIndexOf('\\');

            return path.Substring(0, pos);
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

        public static string GetFolder(string path) {
            int pos = path.LastIndexOf('\\');
            return path.Substring(pos + 1);
        }
        #endregion
    }
}
