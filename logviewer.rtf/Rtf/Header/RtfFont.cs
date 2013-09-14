using System;
using logviewer.rtf.Rtf.Attributes;

namespace logviewer.rtf.Rtf.Header
{
    /// <summary>
    /// Specifies font family.
    /// </summary>
    [RtfEnumAsControlWord(RtfEnumConversion.UseAttribute)]
    public enum RtfFontFamily { 
        [RtfControlWord("fnil")]
        Default,
        [RtfControlWord("froman")]
        Roman,
        [RtfControlWord("fswiss")]
        Swiss,
        [RtfControlWord("fmodern")]
        FixedPitch,
        [RtfControlWord("fscript")]
        Script,
        [RtfControlWord("fdecor")]
        Decorative,
        [RtfControlWord("ftech")]
        Technical,
        [RtfControlWord("fbidi")]
        Bidirectional 
    }
    
    /// <summary>
    /// Specifies character set.
    /// </summary>
    [RtfEnumAsControlWord(RtfEnumConversion.UseValue, Prefix = "fcharset")]
    public enum RtfCharacterSet
    {
        ANSI = 0,
        Default = 1,
        Symbol = 2,
        Invalid = 3,
        Mac = 77,
        ShiftJis = 128,
        Hangul = 129,
        Johab = 130,
        GB2312 = 134,
        Big5 = 136,
        Greek = 161,
        Turkish = 162,
        Vietnamese = 163,
        Hebrew = 177,
        Arabic = 178,
        ArabicTraditional = 179,
        ArabicUser = 180,
        HebrewUser = 181,
        Baltic = 186,
        Russian = 204,
        Thai = 222,
        EasternEuropean = 238,
        PC437 = 254,
        OEM = 255,
    }

    /// <summary>
    /// Specifies font pitch.
    /// </summary>
    [RtfEnumAsControlWord(RtfEnumConversion.UseValue, Prefix = "fprq")]
    public enum RtfFontPitch { Default, Fixed, Variable }

    /// <summary>
    /// Represents a font.
    /// </summary>
    [RtfControlWord("f", IsIndexed = true), RtfEnclosingBraces(ClosingSemicolon = true)]
    public class RtfFont
    {
        private RtfFontFamily fontFamily = RtfFontFamily.Default;
        private RtfCharacterSet characterSet = RtfCharacterSet.Default;
        private RtfFontPitch pitch = RtfFontPitch.Default;
        private string fontName = String.Empty;


        [RtfControlWord]
        public RtfFontFamily FontFamily
        {
            get { return fontFamily; }
            set { fontFamily = value; }
        }

        [RtfControlWord]
        public RtfCharacterSet CharacterSet
        {
            get { return characterSet; }
            set { characterSet = value; }
        }

        [RtfControlWord]
        public RtfFontPitch Pitch
        {
            get { return pitch; }
            set { pitch = value; }
        }

        [RtfTextData]
        public string FontName
        {
            get { return fontName; }
            set { fontName = value; }
        }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFont class.
        /// </summary>
        public RtfFont(string fontName)
        {
            this.fontName = fontName;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFont class.
        /// </summary>
        public RtfFont(string fontName, RtfCharacterSet characterSet)
        {
            this.fontName = fontName;
            this.characterSet = characterSet;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFont class.
        /// </summary>
        public RtfFont(string fontName, RtfCharacterSet characterSet, RtfFontFamily fontFamily)
        {
            this.fontName = fontName;
            this.characterSet = characterSet;
            this.fontFamily = fontFamily;
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfFont class.
        /// </summary>
        public RtfFont(string fontName, RtfCharacterSet characterSet, RtfFontFamily fontFamily, RtfFontPitch pitch)
        {
            this.fontName = fontName;
            this.characterSet = characterSet;
            this.fontFamily = fontFamily;
            this.pitch = pitch;
        }
    }
}