using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using logviewer.core;
using Xunit;

namespace logviewer.tests
{
    public class TstFixedSizeDictionary
    {
        private const string ExpectedString = "str";
        readonly KeyValuePair<int, string> empty = new KeyValuePair<int, string>();

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
        public void AddAndTryGetValueUnexist()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);
            string result;
            instance.TryGetValue(4, out result).Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void AddAndGetValueTrue()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);
            instance[3].Should().Be(ExpectedString);
        }

        [Fact]
        public void GetValueOutsideSize()
        {
            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                new FixedSizeDictionary<string>(10)[11].Should();
            });
        }

        [Fact]
        public void SetValueIntsideSize()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance[2] = ExpectedString;
            instance[2].Should().Be(ExpectedString);
        }

        [Fact]
        public void SetValueOutsideSize()
        {
            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                new FixedSizeDictionary<string>(10)[11] = ExpectedString;
            });
        }

        [Fact]
        public void IsReadOnly()
        {
            new FixedSizeDictionary<string>(10).IsReadOnly.Should().BeFalse();
        }

        [Fact]
        public void CountEmpty()
        {
            new FixedSizeDictionary<string>(10).Count.Should().Be(0);
        }

        [Fact]
        public void CountSingle()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);
            instance.Count.Should().Be(1);
        }

        [Fact]
        public void CountMany()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(2, ExpectedString);
            instance.Add(3, ExpectedString);
            instance.Count.Should().Be(2);
        }

        [Fact]
        public void EnumerateEmpty()
        {
            int iterations = 0;
            // ReSharper disable once UnusedVariable
            foreach (var pair in new FixedSizeDictionary<string>(10))
            {
                iterations++;
            }
            iterations.Should().Be(0);
        }

        [Fact]
        public void EnumerateNotEmpty()
        {
            var instance = new FixedSizeDictionary<string>(10);
            int iterations = 0;
            instance.Add(2, ExpectedString);
            foreach (var pair in instance)
            {
                iterations++;
                pair.Key.Should().Be(2);
                pair.Value.Should().Be(ExpectedString);
            }
            iterations.Should().Be(1);
        }

        [Fact]
        public void EnumerateAsNotGenericEnumerable()
        {
            var instance = new FixedSizeDictionary<string>(10);
            int iterations = 0;
            instance.Add(2, ExpectedString);
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var pair in (IEnumerable)instance)
            {
                iterations++;
            }
            iterations.Should().Be(1);
        }

        [Fact]
        public void ContainsKeyValue()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(2, ExpectedString);
            instance.Contains(new KeyValuePair<int, string>(2, ExpectedString)).Should().BeTrue();
        }

        [Fact]
        public void NotContainsKeyValue()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(2, ExpectedString);
            instance.Contains(new KeyValuePair<int, string>(2, "another")).Should().BeFalse("different value but key the same should not be found");
        }

        [Fact]
        public void AddKeyValuePair()
        {
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(new KeyValuePair<int, string>(2, ExpectedString));
            instance.ContainsKey(2).Should().BeTrue();
        }

        [Fact]
        public void RemoveKeyValuePair()
        {
            var value = new KeyValuePair<int, string>(2, ExpectedString);
            var instance = new FixedSizeDictionary<string>(10) { value };
            instance.Remove(value).Should().BeTrue();
            instance.ContainsKey(2).Should().BeFalse();
        }

        public static IEnumerable<object[]> InstancesToClear => new[]
        {
            new object[] { new FixedSizeDictionary<string>(10) { new KeyValuePair<int, string>(2, ExpectedString) } },
            new object[] { new FixedSizeDictionary<string>(10) }
        };

        [Theory, MemberData("InstancesToClear")]
        public void Clear(FixedSizeDictionary<string> instance)
        {
            var value = new KeyValuePair<int, string>(2, ExpectedString);
            instance.Clear();
            instance.Count.Should().Be(0);
            instance.Remove(value).Should().BeFalse();
            instance.ContainsKey(2).Should().BeFalse();
        }

        [Fact]
        public void CopyToSameSizeFromZero()
        {
            var value = new KeyValuePair<int, string>(1, ExpectedString);
            var instance = new FixedSizeDictionary<string>(2) { value };

            var array = new KeyValuePair<int, string>[2];

            instance.CopyTo(array, 0);

            array.Should().BeEquivalentTo(new[] { this.empty, value });
        }

        [Fact]
        public void CopyToSameSizeFromNonZero()
        {
            var value = new KeyValuePair<int, string>(0, ExpectedString);
            var instance = new FixedSizeDictionary<string>(2) { value };

            var array = new KeyValuePair<int, string>[2];

            instance.CopyTo(array, 1);

            array.Should().BeEquivalentTo(new[] { this.empty, this.empty });
        }

        [Fact]
        public void CopyToSameSizeFromZeroTargetGreaterThenSource()
        {
            var value = new KeyValuePair<int, string>(0, ExpectedString);
            var instance = new FixedSizeDictionary<string>(2) { value };

            var array = new KeyValuePair<int, string>[3];

            instance.CopyTo(array, 0);

            array.Should().BeEquivalentTo(new[] { value, this.empty, this.empty });
        }

        [Fact]
        public void CopyToSameSizeFromZeroTargetLessThenSource()
        {
            var value = new KeyValuePair<int, string>(0, ExpectedString);
            var instance = new FixedSizeDictionary<string>(3) { value };

            var array = new KeyValuePair<int, string>[2];

            instance.CopyTo(array, 0);

            array.Should().BeEquivalentTo(new[] { value, this.empty });
        }

        [Fact]
        public void CopyToSameSizeFromNotZeroTargetLessThenSource()
        {
            var value = new KeyValuePair<int, string>(2, ExpectedString);
            var instance = new FixedSizeDictionary<string>(3) { value };

            var array = new KeyValuePair<int, string>[2];

            instance.CopyTo(array, 1);

            array.Should().BeEquivalentTo(new[] { this.empty, this.empty });
        }
    }
}