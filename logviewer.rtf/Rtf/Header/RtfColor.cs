using System.Drawing;
using logviewer.rtf.Rtf.Attributes;

namespace logviewer.rtf.Rtf.Header
{
    /// <summary>
    /// Represents a color.
    /// </summary>
    [RtfEnclosingBraces(Braces = false, ClosingSemicolon = true)]
    public class RtfColor
    {
        public static readonly RtfColor Auto = new RtfColor(-1, -1, -1);

        [RtfControlWord, RtfInclude(ConditionMember = "IsNotAuto")]
        public int Red { get; set; }

        [RtfControlWord, RtfInclude(ConditionMember = "IsNotAuto")]
        public int Green { get; set; }

        [RtfControlWord, RtfInclude(ConditionMember = "IsNotAuto")]
        public int Blue { get; set; }

        /// <summary>
        /// Condition member used by RtfWriter.
        /// </summary>
        public bool IsNotAuto
        {
            get { return Red >= 0 && Green >= 0 && Blue >= 0; }
        }
        

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfColor class.
        /// </summary>
        public RtfColor()
        {
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfColor class.
        /// </summary>
        public RtfColor(int red, int green, int blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfColor class.
        /// </summary>
        public RtfColor(Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }


        public static explicit operator Color(RtfColor c)
        {
            return Color.FromArgb(c.Red, c.Green, c.Blue);
        }

        public static explicit operator RtfColor(Color c)
        {
            return new RtfColor(c);
        }
    }
}
