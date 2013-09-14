using System;
using logviewer.rtf.Rtf.Attributes;

namespace logviewer.rtf.Rtf.Formatting
{
    /// <summary>
    /// Specifies text align inside a paragraph.
    /// </summary>
    [RtfEnumAsControlWord(RtfEnumConversion.UseAttribute)]
    public enum RtfTextAlign
    {
        [RtfControlWord("ql")]
        Left,
        [RtfControlWord("qc")]
        Center,
        [RtfControlWord("qr")]
        Right,
        [RtfControlWord("qj")]
        Justified,
    }

    /// <summary>
    /// Represents paragraph formatting.
    /// </summary>
    public class RtfParagraphFormatting
    {
        public RtfTextAlign _align = RtfTextAlign.Left;
        public float _fontSize = 12F;
        public int backgroundColorIndex = -1;
        public int textColorIndex = -1;
        public int fontIndex = -1;

        /// <summary>
        /// Default is left.
        /// </summary>
        [RtfControlWord]
        public RtfTextAlign Align
        {
            get { return _align; }
            set { _align = value; }
        }

        /// <summary>
        /// Paragraph first line indent in twips. Default is 0.
        /// </summary>
        [RtfControlWord("fi")]
        public int FirstLineIndent { get; set; }

        /// <summary>
        /// Paragraph left indent in twips. Default is 0.
        /// </summary>
        [RtfControlWord("li")]
        public int IndentLeft { get; set; }

        /// <summary>
        /// Paragraph right indent in twips. Default is 0.
        /// </summary>
        [RtfControlWord("ri")]
        public int IndentRight { get; set; }

        /// <summary>
        /// Gets space between lines in twips. The value is used by RtfWriter.
        /// </summary>
        [RtfControlWord("sl")]
        public int TwipLineSpacing
        {
            get { return (int)Math.Round(LineSpacingPercent * FontSize * TwipConverter.TwipsInPoint); }
        }

        /// <summary>
        /// Space before paragraph in twips. Default is 0.
        /// </summary>
        [RtfControlWord("sb")]
        public int SpaceBefore { get; set; }

        /// <summary>
        /// Space after paragraph in twips. Default is 0.
        /// </summary>
        [RtfControlWord("sa")]
        public int SpaceAfter { get; set; }

        /// <summary>
        /// Index of an entry in the color table. Default is -1 and is ignored by RtfWriter.
        /// </summary>
        [RtfControlWord("cb"), RtfIndex]
        public int BackgroundColorIndex
        {
            get { return backgroundColorIndex; }
            set { backgroundColorIndex = value; }
        }

        /// <summary>
        /// Index of an entry in the color table. Default is -1 and is ignored by RtfWriter.
        /// </summary>
        [RtfControlWord("cf"), RtfIndex]
        public int TextColorIndex
        {
            get { return textColorIndex; }
            set { textColorIndex = value; }
        }

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
        [RtfControlWord("fs")]
        public int HalfPointFontSize
        {
            get { return (int)Math.Round(_fontSize * 2); }
        }

        /// <summary>
        /// Font size in points. Default is 12.
        /// </summary>
        public float FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }

        /// <summary>
        /// Space between lines in percent
        /// . Default is 0 and the space is automatically determined by the tallest character in the line.
        /// </summary>
        public float LineSpacingPercent { get; set; }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfParagraphFormatting class.
        /// </summary>
        public RtfParagraphFormatting()
        {
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfParagraphFormatting class.
        /// </summary>
        /// <param name="fontSize">Font size in points.</param>
        public RtfParagraphFormatting(float fontSize)
        {
            FontSize = fontSize;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfParagraphFormatting class.
        /// </summary>
        /// <param name="align">Text align inside the paragraph.</param>
        public RtfParagraphFormatting(RtfTextAlign align)
        {
            Align = align;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfParagraphFormatting class.
        /// </summary>
        /// <param name="fontSize">Font size in points.</param>
        /// <param name="align">Text align inside the paragraph.</param>
        public RtfParagraphFormatting(float fontSize, RtfTextAlign align)
        {
            FontSize = fontSize;
            Align = align;
        }
    }
}