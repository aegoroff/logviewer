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

        [Theory, MemberData("ValuePairs")]
        public void Add_ValueType_KeysValid(int key, DateTime value)
        {
            // Arrange
            var instance = new FixedSizeDictionary<DateTime>(10);

            // Act
            instance.Add(key, value);

            // Assert
            instance.Keys.ShouldBeEquivalentTo(new[] { key });
        }

        [Theory, MemberData("ReferencePairs")]
        public void Add_ReferenceType_KeysValid(int key, string value)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);

            // Act
            instance.Add(key, value);

            // Assert
            instance.Keys.ShouldBeEquivalentTo(new[] { key });
        }

        [Theory, MemberData("ValuePairs")]
        public void Add_ValueType_ValuesValid(int key, DateTime value)
        {
            // Arrange
            var instance = new FixedSizeDictionary<DateTime>(10);

            // Act
            instance.Add(key, value);

            // Assert
            instance.Values.ShouldBeEquivalentTo(new[] { value });
        }

        [Theory, MemberData("ReferencePairs")]
        public void Add_ReferenceType_ValuesValid(int key, string value)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);

            // Act
            instance.Add(key, value);

            // Assert
            instance.Values.ShouldBeEquivalentTo(new[] { value });
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(ExpectedString)]
        public void ContainsKey_String_True(string str)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, str);

            // Act
            var result = instance.ContainsKey(3);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsKey_AfterAddBeyondSize_False()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(11, string.Empty);

            // Act
            var result = instance.ContainsKey(11);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ContainsKey_AfterRemove_ShouldBeFalse()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, string.Empty);
            var removed = instance.Remove(3);

            // Act
            var result = instance.ContainsKey(3);

            // Assert
            removed.Should().BeTrue();
            result.Should().BeFalse();
        }

        [Fact]
        public void Remove_KeyBeyondSize_ResultFalse()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);

            // Act
            var result = instance.Remove(11);
            
            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void TryGetValue_Exist_True()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);
            string result;
            
            // Act
            var success = instance.TryGetValue(3, out result);

            // Assert
            success.Should().BeTrue();
            result.Should().Be(ExpectedString);
        }

        [Fact]
        public void TryGetValue_Unexist_False()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);
            string result;

            // Act
            var success = instance.TryGetValue(4, out result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void GetValue_Exist_True()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);

            // Act
            var result = instance[3];

            // Assert
            result.Should().Be(ExpectedString);
        }

        [Fact]
        public void GetValue_OutsideSize_Throw()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);

            // Act
            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                var v = instance[11];
            });
        }

        [Fact]
        public void SetValue_InsideSize_ReturnTheSame()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            
            // Act
            instance[2] = ExpectedString;

            // Assert
            instance[2].Should().Be(ExpectedString);
        }

        [Fact]
        public void SetValue_OutsideSize_Throw()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);

            // Act
            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                instance[11] = ExpectedString;
            });
        }

        [Fact]
        public void IsReadOnly_Get_False()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);

            // Act
            var result = instance.IsReadOnly;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Count_EmptyCollection_Zero()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);

            // Act
            var result = instance.Count;

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void Count_SingleElemenCollection_One()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(3, ExpectedString);

            // Act
            var result = instance.Count;

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public void Count_ManyElemenCollection_MoreThenOne()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(2, ExpectedString);
            instance.Add(3, ExpectedString);

            // Act
            var result = instance.Count;

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public void Enumerate_Empty_NoIterations()
        {
            // Arrange
            int iterations = 0;
            
            // Act
            // ReSharper disable once UnusedVariable
            foreach (var pair in new FixedSizeDictionary<string>(10))
            {
                iterations++;
            }

            // Assert
            iterations.Should().Be(0);
        }

        [Fact]
        public void Enumerate_NotEmpty_HasIterations()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            int iterations = 0;
            instance.Add(2, ExpectedString);

            // Act
            foreach (var pair in instance)
            {
                iterations++;
                pair.Key.Should().Be(2);
                pair.Value.Should().Be(ExpectedString);
            }

            // Assert
            iterations.Should().Be(1);
        }

        [Fact]
        public void Enumerate_AsNotGenericEnumerable_HasIterations()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            int iterations = 0;
            instance.Add(2, ExpectedString);

            // Act
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var pair in (IEnumerable)instance)
            {
                iterations++;
            }

            // Assert
            iterations.Should().Be(1);
        }

        [Fact]
        public void Contains_SameKeyValuePair_ShouldBeTrue()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(2, ExpectedString);

            // Act
            var result = instance.Contains(new KeyValuePair<int, string>(2, ExpectedString));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_SameKeyDifferentValue_ShouldBeFalse()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);
            instance.Add(2, ExpectedString);

            // Act
            var result = instance.Contains(new KeyValuePair<int, string>(2, "another"));

            // Assert
            result.Should().BeFalse("different value but key the same should not be found");
        }

        [Fact]
        public void Add_KeyValuePair_Success()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(10);

            // Act
            instance.Add(new KeyValuePair<int, string>(2, ExpectedString));

            // Assert
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
        public void Clear_DifferentCollections(FixedSizeDictionary<string> instance)
        {
            // Arrange
            var value = new KeyValuePair<int, string>(2, ExpectedString);

            // Act
            instance.Clear();

            // Assert
            instance.Count.Should().Be(0);
            instance.Remove(value).Should().BeFalse();
            instance.ContainsKey(2).Should().BeFalse();
        }

        [Fact]
        public void CopyTo_SameSizeFromZero_ResultTheSame()
        {
            // Arrange
            var value = new KeyValuePair<int, string>(1, ExpectedString);
            var instance = new FixedSizeDictionary<string>(2) { value };

            var array = new KeyValuePair<int, string>[2];

            // Act
            instance.CopyTo(array, 0);

            // Assert
            array.Should().BeEquivalentTo(new[] { this.empty, value });
        }

        [Fact]
        public void CopyTo_SameSizeFromNonZero_StrippedBelowIndex()
        {
            // Arrange
            var value = new KeyValuePair<int, string>(0, ExpectedString);
            var instance = new FixedSizeDictionary<string>(2) { value };

            var array = new KeyValuePair<int, string>[2];

            // Act
            instance.CopyTo(array, 1);

            // Assert
            array.Should().BeEquivalentTo(new[] { this.empty, this.empty });
        }

        [Fact]
        public void CopyTo_FromZeroTargetGreaterThenSource_SourceAllWithinTarget()
        {
            // Arrange
            var value = new KeyValuePair<int, string>(0, ExpectedString);
            var instance = new FixedSizeDictionary<string>(2) { value };

            var array = new KeyValuePair<int, string>[3];

            // Act
            instance.CopyTo(array, 0);

            // Assert
            array.Should().BeEquivalentTo(new[] { value, this.empty, this.empty });
        }

        [Fact]
        public void CopyTo_FromZeroTargetLessThenSource_PartOfSourceWithinTarget()
        {
            // Arrange
            var value = new KeyValuePair<int, string>(0, ExpectedString);
            var instance = new FixedSizeDictionary<string>(3) { value };

            var array = new KeyValuePair<int, string>[2];

            // Act
            instance.CopyTo(array, 0);

            // Assert
            array.Should().BeEquivalentTo(new[] { value, this.empty });
        }

        [Fact]
        public void CopyTo_FromNotZeroTargetLessThenSource_NoSourceWithinTarget()
        {
            // Arrange
            var value = new KeyValuePair<int, string>(2, ExpectedString);
            var instance = new FixedSizeDictionary<string>(3) { value };

            var array = new KeyValuePair<int, string>[2];

            // Act
            instance.CopyTo(array, 1);

            // Assert
            array.Should().BeEquivalentTo(new[] { this.empty, this.empty });
        }
    }
}