// Created by: egr
// Created at: 19.09.2012
// © 2012-2013 Alexander Egorov

using logviewer.core;
using NUnit.Framework;

namespace logviewer.tests
{
    [TestFixture]
    internal class TstFileSize
    {
        [TestCase(0UL, SizeUnit.Bytes, 0.0, "0 Bytes", false)]
        [TestCase(1023UL, SizeUnit.Bytes, 0.0, "1023 Bytes", false)]
        [TestCase(1024UL, SizeUnit.KBytes, 1.0, "1,00 Kb (1 024 Bytes)", false)]
        [TestCase(2UL * 1024UL, SizeUnit.KBytes, 2.0, "2,00 Kb (2 048 Bytes)", false)]
        [TestCase(2UL * 1024UL * 1024UL, SizeUnit.MBytes, 2.0, "2,00 Mb (2 097 152 Bytes)", false)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL, SizeUnit.GBytes, 2.0, "2,00 Gb (2 147 483 648 Bytes)", false)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.TBytes, 2.0, "2,00 Tb (2 199 023 255 552 Bytes)", false)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.PBytes, 2.0,
            "2,00 Pb (2 251 799 813 685 248 Bytes)", false)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.EBytes, 2.0,
            "2,00 Eb (2 305 843 009 213 693 952 Bytes)", false)]
        [TestCase(2UL * 1024UL, SizeUnit.KBytes, 2.0, "2,00 Kb", true)]
        public void Normalize(ulong size, SizeUnit unit, double value, string str, bool bigWithoutBytes)
        {
            var sz = new FileSize(size, bigWithoutBytes);
            Assert.That(sz.Bytes, Is.EqualTo(size));
            Assert.That(sz.Unit, Is.EqualTo(unit));
            Assert.That(sz.Value, Is.EqualTo(value));
            Assert.That(sz.ToString(), Is.EqualTo(str));
        }
    }
}