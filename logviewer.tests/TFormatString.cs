// Created by: egr
// Created at: 28.09.2013
// © 2012-2013 Alexander Egorov

using logviewer.core;
using NUnit.Framework;

namespace logviewer.tests
{
    [TestFixture]
    public class TFormatString
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
        public void Test(ulong value, string format)
        {
            Assert.AreEqual(format, value.FormatString());
        }
    }
}