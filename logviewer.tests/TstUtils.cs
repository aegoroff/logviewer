// Created by: egr
// Created at: 24.10.2012
// © 2012-2016 Alexander Egorov

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
        public void ToSafePercent_InRangeSpecified_ResultAsExpected(int value, int min, int max, int result)
        {
            // Act
            var percent = value.ToSafePercent(min, max);

            // Assert
            percent.Should().Be(result);
        }

        [Theory]
        [InlineData(0UL, SizeUnit.Bytes, 0.0, @"0 \w+", false)]
        [InlineData(1023UL, SizeUnit.Bytes, 0.0, @"1023 \w+", false)]
        [InlineData(1024UL, SizeUnit.KBytes, 1.0, @"1[.,]00 \w{2} \(1.024 \w+\)", false)]
        [InlineData(2UL * 1024UL, SizeUnit.KBytes, 2.0, @"2[.,]00 \w{2} \(2.048 \w+\)", false)]
        [InlineData(2UL * 1024UL * 1024UL, SizeUnit.MBytes, 2.0, @"2[.,]00 \w{2} \(2.097.152 \w+\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL, SizeUnit.GBytes, 2.0, @"2[.,]00 \w{2} \(2.147.483.648 \w+\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.TBytes, 2.0, @"2[.,]00 \w{2} \(2.199.023.255.552 \w+\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.PBytes, 2.0,
            @"2[.,]00 \w{2} \(2.251.799.813.685.248 \w+\)", false)]
        [InlineData(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.EBytes, 2.0,
            @"2[.,]00 \w{2}\s\(2.305.843.009.213.693.952 \w+\)", false)]
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
        public void PercentOf_LongType_CalculatedCorrectly(long value, long total, int percent)
        {
            // Act
            var result = value.PercentOf(total);

            // Assert
            result.Should().Be(percent);
        }

        [Fact]
        public void EnsureNoAsyncVoidMethods()
        {
            Helpers.AssertNoAsyncVoidMethods(this.GetType().Assembly);
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