using System;
using logviewer.rtf.Rtf.Attributes;

namespace logviewer.rtf.Rtf.Contents.Text
{
    /// <summary>
    /// Specifies font (character) formatting.
    /// </summary>
    [Flags]
    public enum RtfCharacterFormatting
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Subscript = 8,
        Superscript = 16,
        Caps = 32,
        SmallCaps = 64
    }

    
    /// <summary>
    /// Represents text with formatting.
    /// </summary>
    [RtfEnclosingBraces]
    public class RtfFormattedText : RtfTextBase
    {
        private int fontIndex = -1;
        private float fontSize;
        private int colorIndex = -1;
        private int backgroundColorIndex = -1;
        
        private bool superScript;
        private bool subScript;


        /// <summary>
        /// Index of an entry in the font table. Default is -1 and is ignored by RtfWriter.
        /// </summary>
        [RtfControlWord("f"), RtfIndex]
        public int FontIndex
        {
            get { return fontIndex; }
            set { fontIndex = value; }
        }

        /// <summary>
        /// Gets the font size in half points. Used by RtfWriter.
        /// </summary>
        [RtfControlWord("fs"), RtfInclude(ConditionMember="FontSizeSet")]
        public int HalfPointFontSize
        {
            get { return (int)Math.Round(fontSize * 2); }
        }

        /// <summary>
        /// Index of an entry in the color table. Default is -1 and is ignored by RtfWriter.
        /// </summary>
        [RtfControlWord("cf"), RtfIndex]
        public int TextColorIndex
        {
            get { return colorIndex; }
            set { colorIndex = value; }
        }

        /// <summary>
        /// Index of an entry in the color table. Default is -1 and is ignored by RtfWriter.
        /// </summary>
        [RtfControlWord("cb"), RtfIndex]
        public int BackgroundColorIndex
        {
            get { return backgroundColorIndex; }
            set { backgroundColorIndex = value; }
        }

        [RtfControlWord("b")]
        public bool Bold { get; set; }

        [RtfControlWord("i")]
        public bool Italic { get; set; }

        [RtfControlWord("ul")]
        public bool Underline { get; set; }

        [RtfControlWord("sub")]
        public bool Subscript
        {
            get { return subScript; }
            set 
            {
                if (value)
                {
                    superScript = false;
                }

                subScript = value;
            }
        }
        
        [RtfControlWord("super")]
        public bool Superscript
        {
            get { return superScript; }
            set
            {
                if (value)
                {
                    subScript = false;
                }

                superScript = value;
            }
        }

        [RtfControlWord("caps")]
        public bool Caps { get; set; }

        [RtfControlWord("scaps")]
        public bool SmallCaps { get; set; }
                
        
        /// <summary>
        /// Gets string value of the text.
        /// </summary>
        [RtfTextData]
        public string Text
        {
            get { return sb.ToString(); }
        }
        


        /// <summary>
        /// Font size in points. Default value 0 is ignored by RtfWriter.
        /// </summary>
        public float FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        /// <summary>
        /// ConditionMember used by RtfWriter.
        /// </summary>
        public bool FontSizeSet
        {
            get { return Math.Abs(fontSize) > 0.000001; }
        }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedText class.
        /// </summary>
        public RtfFormattedText()
        {
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedText class.
        /// </summary>
        /// <param name="text">String value to set as text.</param>
        public RtfFormattedText(string text) : base(text)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedText class.
        /// </summary>
        /// <param name="text">String value to set as text.</param>
        /// <param name="colorIndex">Index of an entry in the color table.</param>
        public RtfFormattedText(string text, int colorIndex) : base(text)
        {
            this.colorIndex = colorIndex;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedText class.
        /// </summary>
        /// <param name="formatting">Character formatting to apply to the text.</param>
        public RtfFormattedText(RtfCharacterFormatting formatting)
        {
            SetFormatting(formatting);
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedText class.
        /// </summary>
        /// <param name="text">String value to set as text.</param>
        /// <param name="formatting">Character formatting to apply to the text.</param>
        public RtfFormattedText(string text, RtfCharacterFormatting formatting) : base(text)
        {
            SetFormatting(formatting);
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedText class.
        /// </summary>
        /// <param name="formatting">Character formatting to apply to the text.</param>
        /// <param name="colorIndex">Index of an entry in the color table.</param>
        public RtfFormattedText(RtfCharacterFormatting formatting, int colorIndex)
        {
            SetFormatting(formatting);
            this.colorIndex = colorIndex;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedText class.
        /// </summary>
        /// <param name="text">String value to set as text.</param>
        /// <param name="formatting">Character formatting to apply to the text.</param>
        /// /// <param name="colorIndex">Index of an entry in the color table.</param>
        public RtfFormattedText(string text, RtfCharacterFormatting formatting, int colorIndex) : base(text)
        {
            SetFormatting(formatting);
            this.colorIndex = colorIndex;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedText class.
        /// </summary>
        /// <param name="text">String value to set as text.</param>
        /// <param name="fontIndex">Index of an entry in the font table.</param>
        /// <param name="fontSize"></param>
        public RtfFormattedText(string text, int fontIndex, float fontSize) : base(text)
        {
            this.fontIndex = fontIndex;
            this.fontSize = fontSize;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedText class.
        /// </summary>
        /// <param name="formatting">Character formatting to apply to the text.</param>
        /// <param name="fontIndex">Index of an entry in the font table.</param>
        /// <param name="fontSize"></param>
        public RtfFormattedText(RtfCharacterFormatting formatting, int fontIndex, float fontSize)
        {
            SetFormatting(formatting);
            this.fontSize = fontSize;
            this.fontIndex = fontIndex;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFormattedText class.
        /// </summary>
        /// <param name="text">String value to set as text.</param>
        /// <param name="formatting">Character formatting to apply to the text.</param>
        /// <param name="fontIndex">Index of an entry in the font table.</param>
        /// <param name="fontSize"></param>
        public RtfFormattedText(string text, RtfCharacterFormatting formatting, int fontIndex, float fontSize) : base(text)
        {
            SetFormatting(formatting);
            this.fontSize = fontSize;
            this.fontIndex = fontIndex;
        }


        /// <summary>
        /// Applies specified formatting to the text.
        /// </summary>
        /// <param name="formatting">Character formatting to apply.</param>
        public void SetFormatting(RtfCharacterFormatting formatting)
        {
            Bold = (formatting & RtfCharacterFormatting.Bold) == RtfCharacterFormatting.Bold;
            Italic = (formatting & RtfCharacterFormatting.Italic) == RtfCharacterFormatting.Italic;
            Underline = (formatting & RtfCharacterFormatting.Underline) == RtfCharacterFormatting.Underline;
            Subscript = (formatting & RtfCharacterFormatting.Subscript) == RtfCharacterFormatting.Subscript;
            Superscript = (formatting & RtfCharacterFormatting.Superscript) == RtfCharacterFormatting.Superscript;
            Caps = (formatting & RtfCharacterFormatting.Caps) == RtfCharacterFormatting.Caps;
            SmallCaps = (formatting & RtfCharacterFormatting.SmallCaps) == RtfCharacterFormatting.SmallCaps;
        }
    }
}