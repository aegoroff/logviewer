using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using logviewer.rtf.Rtf.Attributes;
using logviewer.rtf.Rtf.Contents.Text;

namespace logviewer.rtf.Rtf.Contents
{
    [RtfEnumAsControlWord(RtfEnumConversion.UseAttribute)]
    public enum RtfImageFormat
    {
        [RtfControlWord("pngblip")]
        Png,
        [RtfControlWord("jpegblip")]
        Jpeg,
        [RtfControlWord("wmetafile8")]
        Wmf,
    }

    
    /// <summary>
    /// Represents an image.
    /// </summary>
    [RtfControlWord("pict"), RtfEnclosingBraces]
    public class RtfImage : RtfParagraphContentBase
    {
        private const int MillimetersInInch = 2540;

        private Bitmap _bitmap;
        
        private float _dpiX;
        private float _dpiY;

        private int wmfWidth;
        private int wmfHeight;
        
        private int scaleX = 100;
        private int scaleY = 100;

        private string hexData;

        private RtfImageFormat _format = RtfImageFormat.Wmf;


        /// <summary>
        /// Gets the horizontal resolution of the output image.
        /// </summary>
        public float DpiX
        {
            get { return _dpiX; }
        }

        /// <summary>
        /// Gets the vertical resolution of the output image.
        /// </summary>
        public float DpiY
        {
            get { return _dpiY; }
        }

        /// <summary>
        /// Gets horizontal scale. Default is 100.
        /// </summary>
        [RtfControlWord("picscalex")]
        public int ScaleX
        {
            get { return scaleX; }
            set { scaleX = value; }
        }

        /// <summary>
        /// Gets vertical scale. Default is 100.
        /// </summary>
        [RtfControlWord("picscaley")]
        public int ScaleY
        {
            get { return scaleY; }
            set { scaleY= value; }
        }

        /// <summary>
        /// Gets width of the original picture in pixels for bitmaps and millimeters for WMF. Used by RtfWriter.
        /// </summary>
        [RtfControlWord("picw")]
        public int OriginalWidth
        {
            get 
            {
                if (_format == RtfImageFormat.Wmf)
                {
                    return wmfWidth;
                }

                return _bitmap.Width; 
            }
        }

        /// <summary>
        /// Gets height of the original picture in pixels for bitmaps and millimeters for WMF. Used by RtfWriter.
        /// </summary>
        [RtfControlWord("pich")]
        public int OriginalHeight
        {
            get
            {
                if (_format == RtfImageFormat.Wmf)
                {
                    return wmfHeight;
                }

                return _bitmap.Height;
            }
        }

        /// <summary>
        /// Base width of the output image in twips.
        /// </summary>
        [RtfControlWord("picwgoal")]
        public int Width { get; set; }

        /// <summary>
        /// Base height of the output image in twips.
        /// </summary>
        [RtfControlWord("pichgoal")]
        public int Height { get; set; }

        /// <summary>
        /// Format of the output image. Default is Wmf.
        /// </summary>
        [RtfControlWord]
        public RtfImageFormat Format
        {
            get { return _format; }
            set 
            {
                if (_format != value)
                {
                    hexData = String.Empty;
                }

                _format = value; 
            }
        }

        /// <summary>
        /// Gets hexadecimal string representation of the image.
        /// </summary>
        [RtfTextData(RtfTextDataType.Raw)]
        public string HexData
        {
            get 
            {
                if (String.IsNullOrEmpty(hexData))
                {
                    MakeHexData();
                }

                return hexData;
            }
        }


        /// <summary>
        /// Initializes an instance of ESCommon.Rtf.RtfImage class.
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public RtfImage(Bitmap bitmap)
        {
            RtfImageFormat format = RtfImageFormat.Wmf;

            if (bitmap.RawFormat.Equals(ImageFormat.Jpeg))
            {
                format = RtfImageFormat.Jpeg;
            }
            else if (bitmap.RawFormat.Equals(ImageFormat.Png))
            {
                format = RtfImageFormat.Png;
            }
            
            Initialize(bitmap, format);
        }

        /// <param name="format">Image format</param>
        public RtfImage(Bitmap bitmap, RtfImageFormat format)
        {
            Initialize(bitmap, format);
        }

        /// <param name="dpiX">Horizontal resolution</param>
        /// <param name="dpiX">Vertical resolution</param>
        public RtfImage(Bitmap bitmap, RtfImageFormat format, float dpiX, float dpiY)
        {
            Initialize(bitmap, format, dpiX, dpiY);
        }


        private void Initialize(Bitmap bitmap, RtfImageFormat format)
        {
            float dpiX;
            float dpiY;

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                dpiX = graphics.DpiX;
                dpiY = graphics.DpiY;
            }

            Initialize(bitmap, format, dpiX, dpiY);
        }

        private void Initialize(Bitmap bitmap, RtfImageFormat format, float dpiX, float dpiY)
        {
            _bitmap = bitmap;
            _format = format;

            SetDpi(dpiX, dpiY);
        }


        /// <summary>
        /// Resets the size of the bitmap according to DPI value.
        /// </summary>
        public void SetDpi(float dpiX, float dpiY)
        {
            _dpiX = dpiX;
            _dpiY = dpiY;

            Width = (int)Math.Round(_bitmap.Width * TwipConverter.TwipsInInch / dpiX);
            Height = (int)Math.Round(_bitmap.Height * TwipConverter.TwipsInInch / dpiY);

            wmfWidth = (int)Math.Round(_bitmap.Width * MillimetersInInch / dpiX);
            wmfHeight = (int)Math.Round(_bitmap.Height * MillimetersInInch / dpiY);
        }


        private void MakeHexData()
        {
            byte[] data = null;

            if (_format == RtfImageFormat.Wmf)
            {
                data = GetWmfBytes(_bitmap);
            }
            else
            {

                try
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        switch (_format)
                        {
                            case RtfImageFormat.Png:
                                {
                                    _bitmap.Save(stream, ImageFormat.Png);
                                    break;
                                }
                            case RtfImageFormat.Jpeg:
                                {
                                    _bitmap.Save(stream, ImageFormat.Jpeg);
                                    break;
                                }
                        }
                        data = stream.ToArray();
                    }
                }
                catch { }
            }

            if (data != null)
            {
                StringBuilder sb = new StringBuilder(data.Length * 2);
                int i = 0;
                
                foreach (byte b in data)
                {
                    i++;
                    sb.Append(String.Format("{0:x2}", b));
                    
                    if (i == 64)
                    {
                        sb.AppendLine();
                        i = 0;
                    }
                }

                hexData = sb.ToString();
            }
        }


        /// <summary>
        /// Use the EmfToWmfBits function in the GDI+ specification to convert a
        /// Enhanced Metafile to a Windows Metafile
        /// </summary>
        /// <param name="_hEmf">
        /// A handle to the Enhanced Metafile to be converted
        /// </param>
        /// <param name="_bufferSize">
        /// The size of the buffer used to store the Windows Metafile bits returned
        /// </param>
        /// <param name="_buffer">
        /// An array of bytes used to hold the Windows Metafile bits returned
        /// </param>
        /// <param name="_mappingMode">
        /// The mapping mode of the image.  This control uses MM_ANISOTROPIC.
        /// </param>
        /// <param name="_flags">
        /// Flags used to specify the format of the Windows Metafile returned
        /// </param>
        [DllImport("gdiplus.dll", SetLastError = true)]
        static extern uint GdipEmfToWmfBits(IntPtr hEmf, uint uBufferSize, byte[] bBuffer, int iMappingMode, EmfToWmfBitsFlags flags);

        /// <summary>
        /// Releases metafile handle.
        /// </summary>
        /// <param name="_hEmf">
        /// A handle to the Enhanced Metafile to be released.
        /// </param>
        [DllImport("gdi32.dll")]
        private static extern void DeleteEnhMetaFile(IntPtr hEmf);
        
        private static byte[] GetWmfBytes(Bitmap bitmap)
        {
            MemoryStream stream = null;
            Graphics graphics = null;
            Metafile metaFile = null;
            IntPtr hEmf = IntPtr.Zero;
            byte[] data = null;

            try
            {
                using (stream = new MemoryStream())
                {
                    using (graphics = Graphics.FromImage(bitmap))
                    {
                        // Get the device context from the graphics context
                        IntPtr hdc = graphics.GetHdc();

                        // Create a new Enhanced Metafile from the device context
                        metaFile = new Metafile(stream, hdc);

                        // Release the device context
                        graphics.ReleaseHdc(hdc);
                    }

                    // Get a graphics context from the Enhanced Metafile
                    using (graphics = Graphics.FromImage(metaFile))
                    {
                        // Draw the image on the Enhanced Metafile
                        graphics.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                    }

                    using (metaFile)
                    {
                        hEmf = metaFile.GetHenhmetafile();

                        uint bufferSize = GdipEmfToWmfBits(hEmf, 0, null, 8, EmfToWmfBitsFlags.Default);

                        data = new byte[bufferSize];

                        GdipEmfToWmfBits(hEmf, bufferSize, data, 8, EmfToWmfBitsFlags.Default);
                    }
                }
            }
            catch
            {
                data = null;
            }
            finally
            {
                if (hEmf != IntPtr.Zero)
                {
                    DeleteEnhMetaFile(hEmf);
                }

                if (stream != null)
                {
                    stream.Flush();
                    stream.Close();
                }

                if (metaFile != null)
                {
                    metaFile.Dispose();
                }

                if (graphics != null)
                {
                    graphics.Dispose();
                }
            }

            return data;
        }

        private enum EmfToWmfBitsFlags
        {
            // Use the default conversion
            Default = 0x00000000,

            // Embedded the source of the EMF metafiel within the resulting WMF
            // metafile
            EmbedEmf = 0x00000001,

            // Place a 22-byte header in the resulting WMF file.  The header is
            // required for the metafile to be considered placeable.
            IncludePlaceable = 0x00000002,

            // Don't simulate clipping by using the XOR operator.
            NoXORClip = 0x00000004
        } ;
    }
}