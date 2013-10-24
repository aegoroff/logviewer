// Created by: egr
// Created at: 24.10.2012
// © 2012-2013 Alexander Egorov

using logviewer.core;
using NUnit.Framework;

namespace logviewer.tests
{
    [TestFixture]
    public class TUtils
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
    }
}