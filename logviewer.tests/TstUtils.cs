// Created by: egr
// Created at: 24.10.2012
// © 2012-2015 Alexander Egorov

using System.Xml;
using FluentAssertions;
using logviewer.engine;
using logviewer.logic;
using logviewer.logic.support;
using logviewer.logic.ui;
using logviewer.tests.support;
using Xunit;

namespace logviewer.tests
{
    public class TstUtils
    {
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
        [InlineData(0UL, SizeUnit.Bytes, 0.0, @"0 \w+", false)]
        [InlineData(1023UL, SizeUnit.Bytes, 0.0, @"1023 \w+", false)]
        [InlineData(1024UL, SizeUnit.KBytes, 1.0, @"1[.,]00 \w{2} \(1\s024 \w+\)", false)]
        [InlineData(2UL * 1024UL, SizeUnit.KBytes, 2.0, @"2[.,]00 \w{2} \(2\s048 \w+\)", false)]
        [InlineData(2UL * 1024UL * 1024UL, SizeUnit.MBytes, 2.0, @"2[.,]00 \w{2} \(2\s097\s152 \w+\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL, SizeUnit.GBytes, 2.0, @"2[.,]00 \w{2} \(2\s147\s483\s648 \w+\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.TBytes, 2.0, @"2[.,]00 \w{2} \(2\s199\s023\s255\s552 \w+\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.PBytes, 2.0,
            @"2[.,]00 \w{2} \(2\s251\s799\s813\s685\s248 \w+\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.EBytes, 2.0,
            @"2[.,]00\s\w{2}\s\(2\s305\s843\s009\s213\s693\s952 \w+\)", false)]
        [InlineData(2UL * 1024UL, SizeUnit.KBytes, 2.0, @"2[.,]00 \w{2}", true)]
        public void FileSizeNormalize_FormatAndNormalize_NormalizedAndCorrectlyFormatted(ulong size, SizeUnit unit, double value, string pattern, bool bigWithoutBytes)
        {
            // Arrange
            var sz = new FileSize(size, bigWithoutBytes);

            // Act
            var formatted = sz.Format();

            // Assert
            sz.Bytes.Should().Be(size);
            sz.Unit.Should().Be(unit);
            sz.Value.Should().Be(value);
            formatted.Should().MatchRegex(pattern);
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
        public void GenerateKeys_Call_Success()
        {
            // Arrange
            var crypt = new AsymCrypt();

            // Act
            crypt.GenerateKeys();

            // Assert
            crypt.PrivateKey.Should().NotBeNullOrEmpty();
            crypt.PublicKey.Should().NotBeNullOrEmpty();
        }
        
        [Fact]
        public void FromBase64String_GetPlainXml_Success()
        {
            // Arrange
            var crypt = new AsymCrypt();
            crypt.GenerateKeys();

            // Act
            var xml = AsymCrypt.FromBase64String(crypt.PrivateKey);
            var d = new XmlDocument();
            d.LoadXml(xml);
        }
        
        [Fact]
        public void EncryptDecrypt_WithGenerateKeys_CorrectStringAfterDecrypt()
        {
            // Arrange
            const string plain = "string";
            var crypt = new AsymCrypt();

            // Act
            crypt.GenerateKeys();
            var crypted = crypt.Encrypt(plain);
            var decrypted = crypt.Decrypt(crypted);

            // Assert
            decrypted.Should().Be(plain);
        }

        [Theory]
        [InlineData(null, false, true)]
        [InlineData("", false, true)]
        [InlineData(" ", false, true)]
        [InlineData("test", false, true)]
        [InlineData("test", true, true)]
        [InlineData(null, true, true)]
        [InlineData("", true, true)]
        [InlineData(" ", true, true)]
        [InlineData(".*((", true, false)]
        public void IsValid_CorrectString_Success(string filter, bool useRegexp, bool result)
        {
            // Act
            var valid = filter.IsValid(useRegexp);

            // Assert
            valid.Should().Be(result);
        }
    }
}