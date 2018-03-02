// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 25.11.2015
// © 2012-2018 Alexander Egorov

using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using logviewer.logic.Annotations;
using logviewer.logic.support;
using Xunit;

namespace logviewer.tests
{
    public class TstFixedSizeDictionary
    {
        private const string ExpectedString = "str";
        private const int DictionarySize = 10;
        private readonly KeyValuePair<int, string> empty = new KeyValuePair<int, string>();

        [PublicAPI]
        public static IEnumerable<object[]> ValuePairs => new[]
        {
            new object[] { 2, DateTime.Today },
            new object[] { 2, DateTime.MinValue }
        };

        [PublicAPI]
        public static IEnumerable<object[]> ReferencePairs => new[]
        {
            new object[] { 2, ExpectedString },
            new object[] { 2, null },
            new object[] { 0, ExpectedString },
            new object[] { DictionarySize - 1, ExpectedString }
        };

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Ctor_KeyOutOfRange_ArgumentOutOfRangeException(int count)
        {
            // Arrange

            // Act
            Action action = () => new FixedSizeDictionary<string>(count);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory, MemberData(nameof(ValuePairs))]
        public void Add_ValueType_KeysValid(int key, DateTime value)
        {
            // Arrange
            var instance = new FixedSizeDictionary<DateTime>(DictionarySize);

            // Act
            instance.Add(key, value);

            // Assert
            instance.Keys.Should().BeEquivalentTo(key);
        }

        [Theory, MemberData(nameof(ReferencePairs))]
        public void Add_ReferenceType_KeysValid(int key, string value)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);

            // Act
            instance.Add(key, value);

            // Assert
            instance.Keys.Should().BeEquivalentTo(key);
        }

        [Theory, MemberData(nameof(ValuePairs))]
        public void Add_ValueType_ValuesValid(int key, DateTime value)
        {
            // Arrange
            var instance = new FixedSizeDictionary<DateTime>(DictionarySize);

            // Act
            instance.Add(key, value);

            // Assert
            instance.Values.Should().BeEquivalentTo(value);
        }

        [Theory, MemberData(nameof(ReferencePairs))]
        public void Add_ReferenceType_ValuesValid(int key, string value)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);

            // Act
            instance.Add(key, value);

            // Assert
            instance.Values.Should().BeEquivalentTo(value);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(DictionarySize)]
        public void Add_KeyOutOfRange_NothingAdded(int key)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);

            // Act
            instance.Add(key, ExpectedString);

            // Assert
            instance.Count.Should().Be(0);
        }

        [Fact]
        public void Add_SameKeyTwice_SecondAddDoesntChangeFirstAdded()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);

            // Act
            instance.Add(1, ExpectedString); //-V3058
            instance.Add(1, "2");

            // Assert
            instance.Count.Should().Be(1);
            instance[1].Should().Be(ExpectedString);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(ExpectedString)]
        public void ContainsKey_String_True(string str)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            instance.Add(3, str);

            // Act
            var result = instance.ContainsKey(3);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(DictionarySize)]
        public void ContainsKey_IndexOufOfRange_False(int ix)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            instance.Add(2, ExpectedString);

            // Act
            var result = instance.ContainsKey(ix);

            // Assert
            result.Should().BeFalse();
        }

        [Theory, MemberData(nameof(ReferencePairs))]
        public void ContainsKey_KeyInValidRange_True(int key, string value)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            instance.Add(key, value);

            // Act
            var result = instance.ContainsKey(key);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsKey_AfterRemove_ShouldBeFalse()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            instance.Add(3, string.Empty);
            var removed = instance.Remove(3);

            // Act
            var result = instance.ContainsKey(3);

            // Assert
            removed.Should().BeTrue();
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(DictionarySize)]
        public void Remove_KeyBeyondSize_ResultFalse(int key)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);

            // Act
            var result = instance.Remove(key);
            
            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void TryGetValue_Exist_True()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);
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
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            instance.Add(3, ExpectedString);

            // Act
            var success = instance.TryGetValue(4, out string result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void GetValue_Exist_True()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);
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
            var instance = new FixedSizeDictionary<string>(DictionarySize);
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
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            
            // Act
            instance[2] = ExpectedString;

            // Assert
            instance[2].Should().Be(ExpectedString);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(DictionarySize)]
        public void SetValue_OutsideSize_Throw(int key)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            instance.Add(3, ExpectedString);

            // Act
            Action action = () => instance[key] = ExpectedString;

            // Assert
            action.Should().Throw<IndexOutOfRangeException>();
        }

        [Fact]
        public void IsReadOnly_Get_False()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);

            // Act
            var result = instance.IsReadOnly;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Count_EmptyCollection_Zero()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);

            // Act
            var result = instance.Count;

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void Count_SingleElemenCollection_One()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);
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
            var instance = new FixedSizeDictionary<string>(DictionarySize);
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
            var iterations = 0;
            
            // Act
            // ReSharper disable once UnusedVariable
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var pair in new FixedSizeDictionary<string>(DictionarySize))
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
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            var iterations = 0;
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
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            var iterations = 0;
            instance.Add(2, ExpectedString);

            // Act
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once UnusedVariable
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
            var instance = new FixedSizeDictionary<string>(DictionarySize);
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
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            instance.Add(2, ExpectedString);

            // Act
            var result = instance.Contains(new KeyValuePair<int, string>(2, "another"));

            // Assert
            result.Should().BeFalse("different value but key the same should not be found");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(DictionarySize)]
        public void Contains_KeyOutOfRange_ShouldBeFalse(int ix)
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);
            instance.Add(2, ExpectedString);

            // Act
            var result = instance.Contains(new KeyValuePair<int, string>(ix, ExpectedString));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Add_KeyValuePair_Success()
        {
            // Arrange
            var instance = new FixedSizeDictionary<string>(DictionarySize);

            // Act
            instance.Add(new KeyValuePair<int, string>(2, ExpectedString));

            // Assert
            instance.ContainsKey(2).Should().BeTrue();
        }

        [Fact]
        public void RemoveKeyValuePair()
        {
            var value = new KeyValuePair<int, string>(2, ExpectedString);
            var instance = new FixedSizeDictionary<string>(DictionarySize) { value };
            instance.Remove(value).Should().BeTrue();
            instance.ContainsKey(2).Should().BeFalse();
        }

        [PublicAPI]
        public static IEnumerable<object[]> InstancesToClear => new[]
        {
            new object[] { new FixedSizeDictionary<string>(DictionarySize) { new KeyValuePair<int, string>(2, ExpectedString) } },
            new object[] { new FixedSizeDictionary<string>(DictionarySize) }
        };

        [Theory, MemberData(nameof(InstancesToClear))]
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
            array.Should().BeEquivalentTo(this.empty, value);
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
            array.Should().BeEquivalentTo(this.empty, this.empty);
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
            array.Should().BeEquivalentTo(value, this.empty, this.empty);
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
            array.Should().BeEquivalentTo(value, this.empty);
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
            array.Should().BeEquivalentTo(this.empty, this.empty);
        }
    }
}
