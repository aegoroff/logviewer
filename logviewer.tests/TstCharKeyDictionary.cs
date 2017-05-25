// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 26.04.2017
// © 2012-2017 Alexander Egorov

using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using logviewer.engine.strings;
using logviewer.logic.Annotations;
using Xunit;

namespace logviewer.tests
{
    public class TstCharKeyDictionary
    {
        private const string ExpectedString = "str";
        private readonly KeyValuePair<char, string> empty = new KeyValuePair<char, string>();

        [PublicAPI]
        public static IEnumerable<object[]> ValuePairs => new[]
                                                          {
                                                              new object[] { '2', DateTime.Today },
                                                              new object[] { '2', DateTime.MinValue }
                                                          };

        [PublicAPI]
        public static IEnumerable<object[]> ReferencePairs => new[]
                                                              {
                                                                  new object[] { '2', ExpectedString },
                                                                  new object[] { '2', null },
                                                                  new object[] { char.MinValue, ExpectedString },
                                                                  new object[] { char.MaxValue - 1, ExpectedString }
                                                              };

        [Theory, MemberData(nameof(ValuePairs))]
        public void Add_ValueType_KeysValid(char key, DateTime value)
        {
            // Arrange
            var instance = new CharKeyDictionary<DateTime>();

            // Act
            instance.Add(key, value);

            // Assert
            instance.Keys.ShouldBeEquivalentTo(new[] { key });
        }

        [Theory, MemberData(nameof(ReferencePairs))]
        public void Add_ReferenceType_KeysValid(char key, string value)
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();

            // Act
            instance.Add(key, value);

            // Assert
            instance.Keys.ShouldBeEquivalentTo(new[] { key });
        }

        [Theory, MemberData(nameof(ValuePairs))]
        public void Add_ValueType_ValuesValid(char key, DateTime value)
        {
            // Arrange
            var instance = new CharKeyDictionary<DateTime>();

            // Act
            instance.Add(key, value);

            // Assert
            instance.Values.ShouldBeEquivalentTo(new[] { value });
        }

        [Theory, MemberData(nameof(ReferencePairs))]
        public void Add_ReferenceType_ValuesValid(char key, string value)
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();

            // Act
            instance.Add(key, value);

            // Assert
            instance.Values.ShouldBeEquivalentTo(new[] { value });
        }

        [Fact]
        public void Add_SameKeyTwice_SecondAddDoesntChangeFirstAdded()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();

            // Act
            instance.Add('1', ExpectedString); //-V3058
            instance.Add('1', "2");

            // Assert
            instance.Count.Should().Be(1);
            instance['1'].Should().Be(ExpectedString);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(ExpectedString)]
        public void ContainsKey_String_True(string str)
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();
            instance.Add('3', str);

            // Act
            var result = instance.ContainsKey('3');

            // Assert
            result.Should().BeTrue();
        }

        [Theory, MemberData(nameof(ReferencePairs))]
        public void ContainsKey_KeyInValidRange_True(char key, string value)
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();
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
            var instance = new CharKeyDictionary<string>();
            instance.Add('3', string.Empty);
            var removed = instance.Remove('3');

            // Act
            var result = instance.ContainsKey('3');

            // Assert
            removed.Should().BeTrue();
            result.Should().BeFalse();
        }

        [Fact]
        public void TryGetValue_Exist_True()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();
            instance.Add('3', ExpectedString);
            string result;
            
            // Act
            var success = instance.TryGetValue('3', out result);

            // Assert
            success.Should().BeTrue();
            result.Should().Be(ExpectedString);
        }

        [Fact]
        public void TryGetValue_Unexist_False()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();
            instance.Add('3', ExpectedString);

            // Act
            var success = instance.TryGetValue('4', out string result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void GetValue_Exist_True()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();
            instance.Add('3', ExpectedString);

            // Act
            var result = instance['3'];

            // Assert
            result.Should().Be(ExpectedString);
        }

        [Fact]
        public void SetValue_InsideSize_ReturnTheSame()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();
            
            // Act
            instance['2'] = ExpectedString;

            // Assert
            instance['2'].Should().Be(ExpectedString);
        }

        [Fact]
        public void IsReadOnly_Get_False()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();

            // Act
            var result = instance.IsReadOnly;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Count_EmptyCollection_Zero()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();

            // Act
            var result = instance.Count;

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void Count_SingleElemenCollection_One()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();
            instance.Add('3', ExpectedString);

            // Act
            var result = instance.Count;

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public void Count_ManyElemenCollection_MoreThenOne()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();
            instance.Add('2', ExpectedString);
            instance.Add('3', ExpectedString);

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
            foreach (var pair in new CharKeyDictionary<string>())
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
            var instance = new CharKeyDictionary<string>();
            var iterations = 0;
            instance.Add('2', ExpectedString);

            // Act
            foreach (var pair in instance)
            {
                iterations++;
                pair.Key.Should().Be('2');
                pair.Value.Should().Be(ExpectedString);
            }

            // Assert
            iterations.Should().Be(1);
        }

        [Fact]
        public void Enumerate_AsNotGenericEnumerable_HasIterations()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();
            var iterations = 0;
            instance.Add('2', ExpectedString);

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
            var instance = new CharKeyDictionary<string>();
            instance.Add('2', ExpectedString);

            // Act
            var result = instance.Contains(new KeyValuePair<char, string>('2', ExpectedString));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_SameKeyDifferentValue_ShouldBeFalse()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();
            instance.Add('2', ExpectedString);

            // Act
            var result = instance.Contains(new KeyValuePair<char, string>('2', "another"));

            // Assert
            result.Should().BeFalse("different value but key the same should not be found");
        }

        [Fact]
        public void Add_KeyValuePair_Success()
        {
            // Arrange
            var instance = new CharKeyDictionary<string>();

            // Act
            instance.Add(new KeyValuePair<char, string>('2', ExpectedString));

            // Assert
            instance.ContainsKey('2').Should().BeTrue();
        }

        [Fact]
        public void Remove_KeyValuePair_Removed()
        {
            // Arrange
            var value = new KeyValuePair<char, string>('2', ExpectedString);
            var instance = new CharKeyDictionary<string> { value };

            // Act
            instance.Remove(value).Should().BeTrue();

            // Assert
            instance.ContainsKey('2').Should().BeFalse();
        }

        [PublicAPI]
        public static IEnumerable<object[]> InstancesToClear => new[]
                                                                {
                                                                    new object[]
                                                                    {
                                                                        new CharKeyDictionary<string> { new KeyValuePair<char, string>('2', ExpectedString) }
                                                                    },
                                                                    new object[]
                                                                    {
                                                                        new CharKeyDictionary<string>()
                                                                    }
                                                                };

        [Theory, MemberData(nameof(InstancesToClear))]
        public void Clear_DifferentCollections(CharKeyDictionary<string> instance)
        {
            // Arrange
            var value = new KeyValuePair<char, string>('2', ExpectedString);

            // Act
            instance.Clear();

            // Assert
            instance.Count.Should().Be(0);
            instance.Remove(value).Should().BeFalse();
            instance.ContainsKey('2').Should().BeFalse();
        }

        [Fact]
        public void CopyTo_SameSizeFromZero_ResultTheSame()
        {
            // Arrange
            var value = new KeyValuePair<char, string>((char)(char.MinValue + 1), ExpectedString);
            var instance = new CharKeyDictionary<string> { value };

            var array = new KeyValuePair<char, string>[2];

            // Act
            instance.CopyTo(array, 0);

            // Assert
            array.Should().BeEquivalentTo(new[] { this.empty, value });
        }

        [Fact]
        public void CopyTo_SameSizeFromNonZero_StrippedBelowIndex()
        {
            // Arrange
            var value = new KeyValuePair<char, string>(char.MinValue, ExpectedString);
            var instance = new CharKeyDictionary<string> { value };

            var array = new KeyValuePair<char, string>[2];

            // Act
            instance.CopyTo(array, 1);

            // Assert
            array.Should().BeEquivalentTo(new[] { this.empty, this.empty });
        }

        [Fact]
        public void CopyTo_FromZeroTargetGreaterThenSource_SourceAllWithinTarget()
        {
            // Arrange
            var value = new KeyValuePair<char, string>(char.MinValue, ExpectedString);
            var instance = new CharKeyDictionary<string> { value };

            var array = new KeyValuePair<char, string>[3];

            // Act
            instance.CopyTo(array, 0);

            // Assert
            array.Should().BeEquivalentTo(new[] { value, this.empty, this.empty });
        }

        [Fact]
        public void CopyTo_FromZeroTargetLessThenSource_PartOfSourceWithinTarget()
        {
            // Arrange
            var value = new KeyValuePair<char, string>(char.MinValue, ExpectedString);
            var instance = new CharKeyDictionary<string> { value };

            var array = new KeyValuePair<char, string>[2];

            // Act
            instance.CopyTo(array, 0);

            // Assert
            array.Should().BeEquivalentTo(new[] { value, this.empty });
        }

        [Fact]
        public void CopyTo_FromNotZeroTargetLessThenSource_NoSourceWithinTarget()
        {
            // Arrange
            var value = new KeyValuePair<char, string>('2', ExpectedString);
            var instance = new CharKeyDictionary<string> { value };

            var array = new KeyValuePair<char, string>[2];

            // Act
            instance.CopyTo(array, 1);

            // Assert
            array.Should().BeEquivalentTo(new[] { this.empty, this.empty });
        }
    }
}