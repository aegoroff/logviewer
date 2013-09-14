using logviewer.rtf.Rtf.Attributes;

namespace logviewer.rtf.Rtf.Formatting
{
    /// <summary>
    /// Specifies border style.
    /// </summary>
    [RtfEnumAsControlWord(RtfEnumConversion.UseAttribute)]
    public enum RtfBorderStyle
    {
        [RtfControlWord("brdrs")]
        SingleThicknessBorder,
        [RtfControlWord("brdrth")]
        DoubleThicknessBorder,
        [RtfControlWord("brdrsh")]
        ShadowedBorder,
        [RtfControlWord("brdrdb")]
        DoubleBorder,
        [RtfControlWord("brdrdot")]
        DottedBorder,
        [RtfControlWord("brdrdash")]
        DashedBorder,
        [RtfControlWord("brdrhair")]
        HairlineBorder,
        [RtfControlWord("brdrinset")]
        InsetBorder,
        [RtfControlWord("brdrdashsm")]
        DashedBorderSmall,
        [RtfControlWord("brdrdashd")]
        DotDashedBorder,
        [RtfControlWord("brdrdashdd")]
        DotDotDashedBorder,
        [RtfControlWord("brdroutset")]
        OutsetBorder,
        [RtfControlWord("brdrtriple")]
        TripleBorder,
        [RtfControlWord("brdrtnthsg")]
        ThickThinBorderSmall,
        [RtfControlWord("brdrthtnsg")]
        ThinThickBorderSmall,
        [RtfControlWord("brdrtnthtnsg")]
        ThinThickThinBorderSmall,
        [RtfControlWord("brdrtnthmg")]
        ThickThinBorderMedium,
        [RtfControlWord("brdrthtnmg")]
        ThinThickBorderMedium,
        [RtfControlWord("brdrtnthtnmg")]
        ThinThickThinBorderMedium,
        [RtfControlWord("brdrtnthlg")]
        ThickThinBorderLarge,
        [RtfControlWord("brdrthtnlg")]
        ThinThickBorderLarge,
        [RtfControlWord("brdrtnthtnlg")]
        ThinThickThinBorderLarge,
        [RtfControlWord("brdrwavy")]
        WavyBorder,
        [RtfControlWord("brdrwavydb")]
        DoubleWavyBorder,
        [RtfControlWord("brdrdashdotstr")]
        StripedBorder,
        [RtfControlWord("brdremboss")]
        EmbossedBorder,
        [RtfControlWord("brdrengrave")]
        EngravedBorder,
        [RtfControlWord("brdrframe")]
        FrameBorder
    }

    /// <summary>
    /// Represents a border.
    /// </summary>
    public class RtfBorder
    {
        private int _width = 10;
        private RtfBorderStyle _style = RtfBorderStyle.SingleThicknessBorder;
        private int _colorIndex = -1;


        /// <summary>
        /// Border width in twips.
        /// </summary>
        [RtfControlWord("brdrw")]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// Border style.
        /// </summary>
        [RtfControlWord]
        public RtfBorderStyle Style
        {
            get { return _style; }
            set { _style = value; }
        }

        /// <summary>
        /// Index of entry in the color table. Default is -1 and is ignored by RtfWriter.
        /// </summary>
        [RtfControlWord("brdrcf"), RtfIndex]
        public int ColorIndex
        {
            get { return _colorIndex; }
            set { _colorIndex = value; }
        }


        /// <summary>
        /// Sets the properties of the current border.
        /// </summary>
        /// <param name="width">Width in points.</param>
        /// <param name="style">Border style.</param>
        /// <param name="colorIndex">Index of entry in the color table.</param>
        public void SetProperties(float width, RtfBorderStyle style, int colorIndex)
        {
            Width = TwipConverter.ToTwip(width, MetricUnit.Point);
            Style = style;
            ColorIndex = colorIndex;
        }

        /// <summary>
        /// Copy all the properties of the current border to specified ESCommon.Rtf.RtfBorder object.
        /// </summary>
        /// <param name="border">Border object to copy to.</param>
        public void CopyTo(RtfBorder border)
        {
            border.Width = this.Width;
            border.Style = this.Style;
            border.ColorIndex = this.ColorIndex;
        }
    }
}