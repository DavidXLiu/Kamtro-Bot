using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamtro_Bot.Nodes
{
    /// <summary>
    /// This node stores information on a custom color.
    /// -C
    /// </summary>
    public class ColorNode
    {
        public uint R;  // Red Value -C
        public uint G;  // Green Value -C
        public uint B;  // Blue Value -C
        public uint Hex;  // Hexidecimal color code (as an int) -C

        // Color Presets  -C
        public static ColorNode BLACK = new ColorNode(0, 0, 0);  // pure black
        public static ColorNode WHITE = new ColorNode(255, 255, 255);

        /// <summary>
        /// Constructs a Color node based off of RGB values
        /// -C
        /// </summary>
        /// <param name="r">The red value (0-255)</param>
        /// <param name="g">The green value (0-255)</param>
        /// <param name="b">The blue value (0-255)</param>
        public ColorNode(uint r, uint g, uint b) {
            R = r;
            G = g;
            B = b;
            Hex = (r << 16) + (g << 8) + b;  // Construct hex color code -C
        }

        /// <summary>
        /// Constructs a ColorNode object given the hex of the color 
        /// -C
        /// </summary>
        /// <param name="color">The hexidecimal color code</param>
        public ColorNode(uint color) {
            R = (color & 0xFF0000) >> 16;  // fancy bit logic, extracts the red from the color hex -C
            G = (color & 0x00FF00) >> 8;  // extracts the green from the color hex -C
            B = color & 0x0000FF;  // extracts the blue from the color hex -C
            Hex = color;
        }

        public string GetHexAsString() {
            return Hex.ToString("X");  // This converts the integer to a string of it in hexidecimal format. -C
        }
    }
}
