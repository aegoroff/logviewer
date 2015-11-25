using System;
using System.Collections.Generic;

using FluentAssertions;

using logviewer.core;

using Xunit;

namespace logviewer.tests
{
    public class TstFixedSizeDictionary
    {
        private const string ExpectedString = "str";

        public static IEnumerable<object[]> ValuePairs => new[]
        {
            new object[] { 2, DateTime.Today },
            new object[] { 2, DateTime.MinValue }
        };

        public static IEnumerable<object[]> ReferencePairs => new[]
        {
            new object[] { 2, ExpectedString },
            new object[] { 2, null }
        };

        private static void KeysTest<T>(int key, T value)
        {
            var instance = new FixedSizeDictionary<T>(10);
            instance.Add(key, value);
            instance.Keys.ShouldBeEquivalentTo(new[] { key });
        }
        private static void ValuesTest<T>(int key, T value)
        {
            var instance = new FixedSizeDictionary<T>(10);
            instance.Add(key, value);
            instance.Values.ShouldBeEquivalentTo(new[] { value });
        }

        [Theory, MemberData("ValuePairs")]
        public void ValueTypeKeys(int key, DateTime value)
        {
            KeysTest(key, value);
        }

        [Theory, MemberData("ReferencePairs")]
        public void ReferenceTypeKeys(int key, string value)
        {
            KeysTest(key, value);
        }

        [Theory, MemberData("ValuePairs")]
        public void ValueTypeValues(int key, DateTime value)
        {
            ValuesTest(key, value);
        }

        [Theory, MemberData("ReferencePairs")]
        public void ReferenceTypeValues(int key, string value)
        {
            ValuesTest(key, value);
        }

        [Fact]
        public void AddAndContainsKeyTrue()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, string.Empty);
            instance.ContainsKey(3).Should().BeTrue();
        }

        [Fact]
        public void AddAndContainsKeyFalse()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, string.Empty);
            instance.ContainsKey(2).Should().BeFalse();
        }

        [Fact]
        public void AddAndContainsKeyBeyondSize()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(11, string.Empty);
            instance.ContainsKey(11).Should().BeFalse();
        }

        [Fact]
        public void AddRemoveAndContainsKeyTrue()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, string.Empty);
            instance.Remove(3).Should().BeTrue();
            instance.ContainsKey(3).Should().BeFalse();
        }

        [Fact]
        public void RemoveKeyBeyondSize()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Remove(11).Should().BeFalse();
        }

        [Fact]
        public void AddAndTryGetValueTrue()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);
            string result;
            instance.TryGetValue(3, out result).Should().BeTrue();
            result.Should().Be(ExpectedString);
        }

        [Fact]
        public void AddAndGetValueTrue()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);
            instance[3].Should().Be(ExpectedString);
        }

        [Fact]
        public void GetValueOutsidesize()
        {
            var instance = new FixedSizeDictionary<string>(10);
            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                instance[11].Should();
            });
        }
    }
}
