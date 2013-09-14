using logviewer.rtf.Rtf.Attributes;
using logviewer.rtf.Rtf.Header;

namespace logviewer.rtf.Rtf
{
    /// <summary>
    /// Specifies RTF document character set.
    /// </summary>
    [RtfEnumAsControlWord(RtfEnumConversion.UseName)]
    public enum RtfDocumentCharacterSet { ANSI, Mac, PC, PCa }

    /// <summary>
    /// Specifies codepage.
    /// </summary>
    [RtfEnumAsControlWord(RtfEnumConversion.UseValue, Prefix = "ansicpg")]
    public enum RtfCodePage {
        IBM = 437,
        Arabic708 = 708,
        Arabic709 = 709,
        Arabic710 = 710,
        Arabic711 = 711,
        Arabic720 = 720,
        Windows819 = 819,
        EasternEuropean = 852,
        Portuguese = 860,
        Hebrew862 = 862,
        FrenchCanadian = 863,
        Arabic864 = 864,
        Norwegian = 865,
        SovietUnion = 866,
        Thai = 874,
        Japanese = 932,
        SimplifiedChinese = 936,
        Korean = 949,
        TraditionalChinese = 950,
        Windows1250 = 1250,
        Windows1251 = 1251,
        WesternEuropean = 1252,
        Greek = 1253,
        Turkish = 1254,
        Hebrew1255 = 1255,
        Arabic1256 = 1256,
        Baltic = 1257,
        Vietnamese = 1258,
        Johab = 1361,
    }

    /// <summary>
    /// Specifies language.
    /// </summary>
    [RtfEnumAsControlWord(RtfEnumConversion.UseValue, Prefix = "lang")]
    public enum RtfLanguage
    {
        Afrikaans = 1078,
        Albanian = 1052,
        Arabic = 1025,
        ArabicAlgeria = 5121,
        ArabicBahrain = 15361,
        ArabicEgypt = 3073,
        ArabicGeneral = 1,
        ArabicIraq = 2049,
        ArabicJordan = 11265,
        ArabicKuwait = 13313,
        ArabicLebanon = 12289,
        ArabicLibya = 4097,
        ArabicMorocco = 6145,
        ArabicOman = 8193,
        ArabicQatar = 16385,
        ArabicSyria = 10241,
        ArabicTunisia = 7169,
        ArabicUAE = 14337,
        ArabicYemen = 9217,
        Armenian = 1067,
        Assamese = 1101,
        AzeriCyrillic = 2092,
        AzeriLatin = 1068,
        Basque = 1069,
        Bengali = 1093,
        BosniaHerzegovina = 4122,
        Bulgarian = 1026,
        Burmese = 1109,
        Byelorussian = 1059,
        Catalan = 1027,
        ChineseChina = 2052,
        ChineseGeneral = 4,
        ChineseHongKong = 3076,
        ChineseMacao = 3076,
        ChineseSingapore = 4100,
        ChineseTaiwan = 1028,
        Croatian = 1050,
        Czech = 1029,
        Danish = 1030,
        DutchBelgium = 2067,
        DutchStandard = 1043,
        EnglishAustralia = 3081,
        EnglishBelize = 10249,
        EnglishBritish = 2057,
        EnglishCanada = 4105,
        EnglishCaribbean = 9225,
        EnglishGeneral = 9,
        EnglishIreland = 6153,
        EnglishJamaica = 8201,
        EnglishNewZealand = 5129,
        EnglishPhilippines = 13321,
        EnglishSouthAfrica = 7177,
        EnglishTrinidad = 11273,
        EnglishUnitedStates = 1033,
        EnglishZimbabwe = 1033,
        Estonian = 1061,
        Faeroese = 1080,
        Farsi = 1065,
        Finnish = 1035,
        French = 1036,
        FrenchBelgium = 2060,
        FrenchCameroon = 11276,
        FrenchCanada = 3084,
        FrenchCoteDIvoire = 12300,
        FrenchLuxemburg = 5132,
        FrenchMali = 13324,
        FrenchMonaco = 6156,
        FrenchReunion = 8204,
        FrenchSenegal = 10252,
        FrenchSwiss = 4108,
        FrenchWestIndies = 7180,
        FrenchZaire = 9228,
        Frisian = 1122,
        Gaelic = 1084,
        GaelicIreland = 2108,
        Galician = 1110,
        Georgian = 1079,
        German = 1031,
        GermanAustrian = 3079,
        GermanLiechtenstein = 5127,
        GermanLuxemburg = 4103,
        GermanSwitzerland = 2055,
        Greek = 1032,
        Gujarati = 1095,
        Hebrew = 1037,
        Hindi = 1081,
        Hungarian = 1038,
        Icelandic = 1039,
        Indonesian = 1057,
        Italian = 1040,
        ItalianSwitzerland = 2064,
        Japanese = 1041,
        Kannada = 1099,
        Kashmiri = 1120,
        KashmiriIndia = 2144,
        Kazakh = 1087,
        Khmer = 1107,
        Kirghiz = 1088,
        Konkani = 1111,
        Korean = 1042,
        KoreanJohab = 2066,
        Lao = 1108,
        Latvian = 1062,
        Lithuanian = 1063,
        LithuanianClassic = 2087,
        Macedonian = 1086,
        Malay = 1086,
        MalayBruneiDarussalam = 2110,
        Malayalam = 1100,
        Maltese = 1082,
        Manipuri = 1112,
        Marathi = 1102,
        Mongolian = 1104,
        Nepali = 1121,
        NepaliIndia = 2145,
        NorwegianBokmal = 1044,
        NorwegianNynorsk = 2068,
        Oriya = 1096,
        Polish = 1045,
        PortugueseBrazil = 1046,
        PortugueseIberian = 2070,
        Punjabi = 1094,
        RhaetoRomanic = 1047,
        Romanian = 1048,
        RomanianMoldova = 2072,
        Russian = 1049,
        RussianMoldova = 2073,
        SamiLappish = 1083,
        Sanskrit = 1103,
        SerbianCyrillic = 3098,
        SerbianLatin = 2074,
        Sindhi = 1113,
        Slovak = 1051,
        Slovenian = 1060,
        Sorbian = 1070,
        SpanishArgentina = 11274,
        SpanishBolivia = 16394,
        SpanishChile = 13322,
        SpanishColombia = 9226,
        SpanishCostaRica = 5130,
        SpanishDominicanRepublic = 7178,
        SpanishEcuador = 12298,
        SpanishElSalvador = 17418,
        SpanishGuatemala = 4106,
        SpanishHonduras = 18442,
        SpanishMexico = 2058,
        SpanishModern = 3082,
        SpanishNicaragua = 19466,
        SpanishPanama = 6154,
        SpanishParaguay = 15370,
        SpanishPeru = 10250,
        SpanishPuertoRico = 20490,
        SpanishTraditional = 1034,
        SpanishUruguay = 14346,
        SpanishVenezuela = 8202,
        Sutu = 1072,
        Swahili = 1089,
        Swedish = 1053,
        SwedishFinland = 2077,
        Tajik = 1064,
        Tamil = 1097,
        Tatar = 1092,
        Telugu = 1098,
        Thai = 1054,
        Tibetan = 1105,
        Tsonga = 1073,
        Tswana = 1074,
        Turkish = 1055,
        Turkmen = 1090,
        Ukrainian = 1058,
        Urdu = 1056,
        UrduIndia = 2080,
        UzbekCyrillic = 2115,
        UzbekLatin = 1091,
        Venda = 1075,
        Vietnamese = 1066,
        Welsh = 1106,
        Xhosa = 1076,
        Yiddish = 1085,
        Zulu = 1077,
    }

    /// <summary>
    /// Represents a RTF document.
    /// </summary>
    [RtfControlWord("rtf1"), RtfEnclosingBraces]
    public class RtfDocument
    {
        private RtfDocumentCharacterSet charSet = RtfDocumentCharacterSet.ANSI;
        private RtfCodePage codePage = RtfCodePage.WesternEuropean;
        private RtfLanguage defaultLanguage = RtfLanguage.EnglishUnitedStates;
        private readonly RtfFontCollection fontTable = new RtfFontCollection();
        private readonly RtfColorCollection colorTable = new RtfColorCollection();
        private readonly RtfDocumentContentCollection contents;


        [RtfControlWord]
        public RtfDocumentCharacterSet CharSet
        {
            get { return charSet; }
            set { charSet = value; }
        }

        [RtfControlWord]
        public RtfCodePage CodePage
        {
            get { return codePage; }
            set { codePage = value; }
        }

        [RtfControlWord("deffont"), RtfIndex, RtfInclude(ConditionMember = "FontTableIsNotEmpty")]
        public int DefaultFont { get; set; }

        [RtfControlWord("deflang")]
        public RtfLanguage DefaultLanguage
        {
            get { return defaultLanguage; }
            set { defaultLanguage = value; }
        }

        [RtfControlGroup("fonttbl"), RtfInclude(ConditionMember = "FontTableIsNotEmpty")]
        public RtfFontCollection FontTable
        {
            get { return fontTable; }
        }

        [RtfControlGroup("colortbl")]
        public RtfColorCollection ColorTable
        {
            get { return colorTable; }
        }

        [RtfInclude]
        public RtfDocumentContentCollection Contents
        {
            get { return contents; }
        }

        public bool FontTableIsNotEmpty
        {
            get { return FontTable != null && FontTable.Count > 0; }
        }


        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfDocument class.
        /// </summary>
        public RtfDocument()
        {
            contents = new RtfDocumentContentCollection(this);
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfDocument class.
        /// </summary>
        /// <param name="codePage">Document codepage.</param>
        public RtfDocument(RtfCodePage codePage) : this()
        {
            CodePage = codePage;
        }
    }
}