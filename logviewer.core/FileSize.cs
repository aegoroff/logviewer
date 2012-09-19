using System;

namespace logviewer.core
{
    /// <summary>
    ///     Represents file size normalization code
    /// </summary>
    public struct FileSize
    {
        private const int Int64BitsCount = 64;
        private const int BinaryThousand = 1024;
        private readonly ulong bytes;
        private readonly SizeUnit sizeUnit;
        private readonly double value;

        /// <summary>
        ///     Initializes new instance of <see cref = "FileSize" /> structure using raw file size
        /// </summary>
        /// <param name = "bytes">File size in bytes</param>
        public FileSize(ulong bytes)
        {
            this.bytes = bytes;
            this.sizeUnit = SizeUnit.Bytes;
            this.sizeUnit = bytes == 0
                                ? SizeUnit.Bytes
                                : (SizeUnit) (IntegerLogarithm(bytes) / IntegerLogarithm(BinaryThousand));
            this.value = this.sizeUnit == SizeUnit.Bytes ? 0 : bytes / Math.Pow(BinaryThousand, (int) this.sizeUnit);
        }

        /// <summary>
        ///     Gets file size in bytes
        /// </summary>
        public ulong Bytes
        {
            get { return this.bytes; }
        }

        /// <summary>
        ///     Gets the unit of value property
        /// </summary>
        public SizeUnit Unit
        {
            get { return this.sizeUnit; }
        }

        /// <summary>
        ///     Gets normalized size defined by Unit property
        /// </summary>
        public double Value
        {
            get { return this.value; }
        }

        private static ulong IntegerLogarithm(ulong x)
        {
            ulong n = Int64BitsCount;
            int c = Int64BitsCount / 2;

            do
            {
                ulong y = x >> c;
                if (y != 0)
                {
                    n -= (ulong) c;
                    x = y;
                }
                c >>= 1;
            } while (c != 0);
            n -= x >> (Int64BitsCount - 1);
            return (Int64BitsCount - 1) - (n - x);
        }
    }
}