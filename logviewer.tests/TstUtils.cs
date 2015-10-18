// Created by: egr
// Created at: 24.10.2012
// © 2012-2015 Alexander Egorov

using System.Xml;
using FluentAssertions;
using logviewer.core;
using logviewer.engine;
using Xunit;

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
            value.FormatString().Should().Be(format);
        }

        [Theory]
        [InlineData(1, 0, 100, 1)]
        [InlineData(0, 0, 100, 0)]
        [InlineData(-1, 0, 100, 0)]
        [InlineData(100, 0, 100, 100)]
        [InlineData(101, 0, 100, 100)]
        public void TestSafePercent(int value, int min, int max, int result)
        {
            value.ToSafePercent(min, max).Should().Be(result);
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
            sz.Bytes.Should().Be(size);
            sz.Unit.Should().Be(unit);
            sz.Value.Should().Be(value);
            sz.Format().Should().MatchRegex(str);
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
            value.PercentOf(total).Should().Be(percent);
        }

        [Fact]
        public void EnsureNoAsyncVoidMethods()
        {
            Helpers.AssertNoAsyncVoidMethods(GetType().Assembly);
            Helpers.AssertNoAsyncVoidMethods(typeof(MainController).Assembly);
        }
        
        [Fact]
        public void GenerateRsaKeys()
        {
            var crypt = new AsymCrypt();
            crypt.GenerateKeys();
            crypt.PrivateKey.Should().NotBeNullOrEmpty();
            crypt.PublicKey.Should().NotBeNullOrEmpty();
        }
        
        [Fact]
        public void DecryptBase64()
        {
            var crypt = new AsymCrypt();
            crypt.GenerateKeys();
            var xml = AsymCrypt.FromBase64String(crypt.PrivateKey);
            var d = new XmlDocument();
            d.LoadXml(xml);
        }
        
        [Fact]
        public void EncryptDecrypt()
        {
            var crypt = new AsymCrypt();
            crypt.GenerateKeys();
            const string plain = "string";
            var crypted = crypt.Encrypt(plain);
            var decrypted = crypt.Decrypt(crypted);
            decrypted.Should().Be(plain);
        }
    }
}