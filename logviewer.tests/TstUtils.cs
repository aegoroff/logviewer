// Created by: egr
// Created at: 24.10.2012
// © 2012-2014 Alexander Egorov

using logviewer.core;
using NUnit.Framework;

namespace logviewer.tests
{
    [TestFixture]
    public class TstUtils
    {
        [TestCase(0UL, "")]
        [TestCase(1UL, "#")]
        [TestCase(11UL, "##")]
        [TestCase(111UL, "###")]
        [TestCase(1111UL, "# ###")]
        [TestCase(11111UL, "## ###")]
        [TestCase(111111UL, "### ###")]
        [TestCase(1111111UL, "# ### ###")]
        [TestCase(11111111UL, "## ### ###")]
        [TestCase(111111111UL, "### ### ###")]
        [TestCase(1111111111UL, "# ### ### ###")]
        [TestCase(11111111111UL, "## ### ### ###")]
        [TestCase(111111111111UL, "### ### ### ###")]
        [TestCase(1111111111111UL, "# ### ### ### ###")]
        [TestCase(11111111111111UL, "## ### ### ### ###")]
        [TestCase(111111111111111UL, "### ### ### ### ###")]
        [TestCase(1111111111111111UL, "# ### ### ### ### ###")]
        [TestCase(11111111111111111UL, "## ### ### ### ### ###")]
        [TestCase(111111111111111111UL, "### ### ### ### ### ###")]
        [TestCase(1111111111111111111UL, "# ### ### ### ### ### ###")]
        [TestCase(ulong.MaxValue, "## ### ### ### ### ### ###")]
        public void TestFormatString(ulong value, string format)
        {
            Assert.AreEqual(format, value.FormatString());
        }

        [TestCase(1, 0, 100, 1)]
        [TestCase(0, 0, 100, 0)]
        [TestCase(-1, 0, 100, 0)]
        [TestCase(100, 0, 100, 100)]
        [TestCase(101, 0, 100, 100)]
        public void TestSafePercent(int value, int min, int max, int result)
        {
            Assert.AreEqual(result, value.ToSafePercent(min, max));
        }

        [TestCase(0UL, SizeUnit.Bytes, 0.0, "0 \\w+", false)]
        [TestCase(1023UL, SizeUnit.Bytes, 0.0, "1023 \\w+", false)]
        [TestCase(1024UL, SizeUnit.KBytes, 1.0, "1,00 \\w{2} \\(1 024 \\w+\\)", false)]
        [TestCase(2UL * 1024UL, SizeUnit.KBytes, 2.0, "2,00 \\w{2} \\(2 048 \\w+\\)", false)]
        [TestCase(2UL * 1024UL * 1024UL, SizeUnit.MBytes, 2.0, "2,00 \\w{2} \\(2 097 152 \\w+\\)", false)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL, SizeUnit.GBytes, 2.0, "2,00 \\w{2} \\(2 147 483 648 \\w+\\)", false)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.TBytes, 2.0, "2,00 \\w{2} \\(2 199 023 255 552 \\w+\\)", false)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.PBytes, 2.0,
            "2,00 \\w{2} \\(2 251 799 813 685 248 \\w+\\)", false)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.EBytes, 2.0,
            "2,00 \\w{2} \\(2 305 843 009 213 693 952 \\w+\\)", false)]
        [TestCase(2UL * 1024UL, SizeUnit.KBytes, 2.0, "2,00 \\w{2}", true)]
        public void TestFileSizeNormalize(ulong size, SizeUnit unit, double value, string str, bool bigWithoutBytes)
        {
            var sz = new FileSize(size, bigWithoutBytes);
            Assert.That(sz.Bytes, Is.EqualTo(size));
            Assert.That(sz.Unit, Is.EqualTo(unit));
            Assert.That(sz.Value, Is.EqualTo(value));
            Assert.That(sz.ToString(), Is.StringMatching(str));
        }

        [TestCase(0L, 100L, 0)]
        [TestCase(15L, 100L, 15)]
        [TestCase(50L, 100L, 50)]
        [TestCase(100L, 100L, 100)]
        [TestCase(200L, 100L, 200)]
        [TestCase(10L, 33L, 30)]
        [TestCase(5L, 33L, 15)]
        [TestCase(-15L, -100L, 0)]
        [TestCase(-15L, 100L, 0)]
        [TestCase(5L, 0L, 0)]
        public void TestPercentOf(long value, long total, int percent)
        {
            Assert.That(value.PercentOf(total), Is.EqualTo(percent));
        }
    }
}