using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// This is the object for a title.
    /// -C
    /// </summary>
    public class TitleNode
    {
        // Something important to note about the Newtonsoft JSON serializer
        // It will not serialize static variables unless specifically told to
        // This is very useful, because it means that this Dictionary won't be 
        // serialized into every title node entry
        // -C
        /// <summary>
        /// This variable stores title nodes as values, and their corresponding IDs as keys.
        /// Dictionaries serialize quite well into JSON.
        /// </summary>
        public static Dictionary<int, TitleNode> NodeMap;  // This is the map that stores the nodes and their corresponding ID values

        public string Name;  // The name of the title -C
        public ColorNode Color;  // The color of the title -C
        
        /// <summary>
        /// This is the constructor for a TitleNode.
        /// This creates a new Title, and registers it.
        /// </summary>
        /// <param name="id">The ID of the</param>
        /// <param name="title"></param>
        /// <param name="color"></param>
        public TitleNode(int id, string title, ColorNode color) {
            Name = title;  // set the name
            Color = color; // set the color
            NodeMap.Add(id, this);  // Add it to the list of nodes.
        }

        public static void SaveNodeMap() {
            BotUtils.WriteToJson(NodeMap, DataFileNames.TitleListFile);  // Save the file
        }
    }
}
