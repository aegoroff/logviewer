// Created by: egr
// Created at: 24.10.2012
// © 2012-2014 Alexander Egorov

using System.Text.RegularExpressions;
using logviewer.core;
using logviewer.engine;
using Xunit;
using Xunit.Extensions;

namespace logviewer.tests
{
    public class TstUtils
    {
        [Theory]
        [InlineData(0UL, "")]
        [InlineData(1UL, "#")]
        [InlineData(11UL, "##")]
        [InlineData(111UL, "###")]
        [InlineData(1111UL, "# ###")]
        [InlineData(11111UL, "## ###")]
        [InlineData(111111UL, "### ###")]
        [InlineData(1111111UL, "# ### ###")]
        [InlineData(11111111UL, "## ### ###")]
        [InlineData(111111111UL, "### ### ###")]
        [InlineData(1111111111UL, "# ### ### ###")]
        [InlineData(11111111111UL, "## ### ### ###")]
        [InlineData(111111111111UL, "### ### ### ###")]
        [InlineData(1111111111111UL, "# ### ### ### ###")]
        [InlineData(11111111111111UL, "## ### ### ### ###")]
        [InlineData(111111111111111UL, "### ### ### ### ###")]
        [InlineData(1111111111111111UL, "# ### ### ### ### ###")]
        [InlineData(11111111111111111UL, "## ### ### ### ### ###")]
        [InlineData(111111111111111111UL, "### ### ### ### ### ###")]
        [InlineData(1111111111111111111UL, "# ### ### ### ### ### ###")]
        [InlineData(ulong.MaxValue, "## ### ### ### ### ### ###")]
        public void TestFormatString(ulong value, string format)
        {
            Assert.Equal(format, value.FormatString());
        }

        [Theory]
        [InlineData(1, 0, 100, 1)]
        [InlineData(0, 0, 100, 0)]
        [InlineData(-1, 0, 100, 0)]
        [InlineData(100, 0, 100, 100)]
        [InlineData(101, 0, 100, 100)]
        public void TestSafePercent(int value, int min, int max, int result)
        {
            Assert.Equal(result, value.ToSafePercent(min, max));
        }

        [Theory]
        [InlineData(0UL, SizeUnit.Bytes, 0.0, "0 \\w+", false)]
        [InlineData(1023UL, SizeUnit.Bytes, 0.0, "1023 \\w+", false)]
        [InlineData(1024UL, SizeUnit.KBytes, 1.0, "1[.,]00 \\w{2} \\(1 024 \\w+\\)", false)]
        [InlineData(2UL * 1024UL, SizeUnit.KBytes, 2.0, "2[.,]00 \\w{2} \\(2 048 \\w+\\)", false)]
        [InlineData(2UL * 1024UL * 1024UL, SizeUnit.MBytes, 2.0, "2[.,]00 \\w{2} \\(2 097 152 \\w+\\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL, SizeUnit.GBytes, 2.0, "2[.,]00 \\w{2} \\(2 147 483 648 \\w+\\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.TBytes, 2.0, "2[.,]00 \\w{2} \\(2 199 023 255 552 \\w+\\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.PBytes, 2.0,
            "2[.,]00 \\w{2} \\(2 251 799 813 685 248 \\w+\\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.EBytes, 2.0,
            "2[.,]00 \\w{2} \\(2 305 843 009 213 693 952 \\w+\\)", false)]
        [InlineData(2UL * 1024UL, SizeUnit.KBytes, 2.0, "2[.,]00 \\w{2}", true)]
        public void TestFileSizeNormalize(ulong size, SizeUnit unit, double value, string str, bool bigWithoutBytes)
        {
            var sz = new FileSize(size, bigWithoutBytes);
            Assert.Equal(size, sz.Bytes);
            Assert.Equal(unit, sz.Unit);
            Assert.Equal(value, sz.Value);
            Assert.True(Regex.IsMatch(sz.Format(), str));
        }

        [Theory]
        [InlineData(0L, 100L, 0)]
        [InlineData(15L, 100L, 15)]
        [InlineData(50L, 100L, 50)]
        [InlineData(100L, 100L, 100)]
        [InlineData(200L, 100L, 200)]
        [InlineData(10L, 33L, 30)]
        [InlineData(5L, 33L, 15)]
        [InlineData(-15L, -100L, 0)]
        [InlineData(-15L, 100L, 0)]
        [InlineData(5L, 0L, 0)]
        public void TestPercentOf(long value, long total, int percent)
        {
            Assert.Equal(percent, value.PercentOf(total));
        }

        [Fact]
        public void EnsureNoAsyncVoidMethods()
        {
            Helpers.AssertNoAsyncVoidMethods(GetType().Assembly);
            Helpers.AssertNoAsyncVoidMethods(typeof(MainController).Assembly);
        }
    }
}