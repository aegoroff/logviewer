using System;
using System.Drawing;

namespace logviewer.logic.ui.main
{
    public class TextFormat
    {
        public string Font { get; set; } = @"Courier New";

        public string SizeHead { get; set; } = "13";

        public string SizeBody { get; set; } = "12";

        public Color Color { get; set; } = Color.Black;

        public string ColorAsString { get; set; }

        public string ColorToString()
        {
            byte[] data = { this.Color.R, this.Color.G, this.Color.B };
            return "#" + BitConverter.ToString(data).Replace("-", "");
        }
    }
}