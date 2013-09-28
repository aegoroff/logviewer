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
        [TestCase(1L, "#")]
        [TestCase(11L, "##")]
        [TestCase(111L, "###")]
        [TestCase(1111L, "# ###")]
        [TestCase(11111L, "## ###")]
        [TestCase(111111L, "### ###")]
        [TestCase(1111111L, "# ### ###")]
        [TestCase(11111111L, "## ### ###")]
        [TestCase(111111111L, "### ### ###")]
        [TestCase(1111111111L, "# ### ### ###")]
        [TestCase(11111111111L, "## ### ### ###")]
        [TestCase(111111111111L, "### ### ### ###")]
        [TestCase(1111111111111L, "# ### ### ### ###")]
        [TestCase(11111111111111L, "## ### ### ### ###")]
        [TestCase(111111111111111L, "### ### ### ### ###")]
        [TestCase(1111111111111111L, "# ### ### ### ### ###")]
        [TestCase(11111111111111111L, "## ### ### ### ### ###")]
        [TestCase(111111111111111111L, "### ### ### ### ### ###")]
        public void Test(long value, string format)
        {
            Assert.AreEqual(format, value.FormatString());
        }
    }
}