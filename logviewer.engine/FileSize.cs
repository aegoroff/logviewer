// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 19.09.2012
// © 2012-2018 Alexander Egorov

using System;

namespace logviewer.engine
{
    /// <summary>
    ///     Represents file size normalization code
    /// </summary>
    public readonly struct FileSize : IEquatable<FileSize>
    {
        private const int Int64BitsCount = 64;

        private const int BinaryThousand = 1024;

        private readonly bool bigWithoutBytes;

        private readonly ulong bytes;

        private readonly SizeUnit sizeUnit;

        private readonly double value;

        /// <summary>
        ///     Initializes new instance of <see cref="FileSize" /> structure using raw file size
        /// </summary>
        /// <param name="bytes">File size in bytes (signed)</param>
        /// <param name="bigWithoutBytes">Whether to output bytes info if size is greater then 1024 bytes. False by default</param>
        public FileSize(long bytes, bool bigWithoutBytes = false) : this((ulong)bytes, bigWithoutBytes)
        {
        }

        /// <summary>
        ///     Initializes new instance of <see cref="FileSize" /> structure using raw file size
        /// </summary>
        /// <param name="bytes">File size in bytes</param>
        /// <param name="bigWithoutBytes">Whether to output bytes info if size is greater then 1024 bytes. False by default</param>
        public FileSize(ulong bytes, bool bigWithoutBytes = false)
        {
            this.bytes = bytes;
            this.bigWithoutBytes = bigWithoutBytes;
            this.sizeUnit = SizeUnit.Bytes;
            this.sizeUnit = bytes == 0
                                ? SizeUnit.Bytes
                                : (SizeUnit)(IntegerLogarithm(bytes) / IntegerLogarithm(BinaryThousand));
            this.value = this.sizeUnit == SizeUnit.Bytes ? 0 : bytes / Math.Pow(BinaryThousand, (int)this.sizeUnit);
        }

        /// <summary>
        ///     Gets file size in bytes
        /// </summary>
        public ulong Bytes => this.bytes;

        /// <summary>
        ///     Gets the unit of value property
        /// </summary>
        public SizeUnit Unit => this.sizeUnit;

        /// <summary>
        ///     Gets normalized size defined by Unit property
        /// </summary>
        public double Value => this.value;

        /// <summary>
        /// Whether to output big values (> 1Kb) without bytes info
        /// </summary>
        public bool BigWithoutBytes => this.bigWithoutBytes;

        private static ulong IntegerLogarithm(ulong x)
        {
            ulong n = Int64BitsCount;
            var c = Int64BitsCount / 2;

            do
            {
                var y = x >> c;
                if (y != 0)
                {
                    n -= (ulong)c;
                    x = y;
                }
                c >>= 1;
            } while (c != 0);

            n -= x >> (Int64BitsCount - 1);
            return (Int64BitsCount - 1) - (n - x);
        }

        
        /// <summary>
        /// Equals FileSize to other
        /// </summary>
        /// <param name="other">Other instance to compare</param>
        /// <returns>True if file size the same, false otherwise</returns>
        public bool Equals(FileSize other) => this.bytes == other.bytes;

        /// <summary>
        /// Equals FileSize to other
        /// </summary>
        /// <param name="obj">Other instance to compare</param>
        /// <returns>True if file size the same, false otherwise</returns>
        public override bool Equals(object obj) => obj is FileSize other && this.Equals(other);

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode() => this.bytes.GetHashCode();
    }
}
