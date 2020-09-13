// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 26.04.2017
// © 2012-2018 Alexander Egorov

using System.Collections.Generic;
using FluentAssertions;
using logviewer.engine.strings;
using Xunit;

namespace logviewer.tests
{
    public class TstAhoCorasickTree
    {
        private const string TestString =
            "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

        [Fact]
        public void Contains_SeveralKvalsSomeInTarget_ReturnTrue()
        {
            // Arrange
            var keywords = new[]
            {
                new KeyValuePair<string, int>("Windows", 1),
                new KeyValuePair<string, int>("Linux", 2),
                new KeyValuePair<string, int>("MacOSX", 3)
            };
            var tree = new AhoCorasickTree<int>(keywords);

            // Act
            var result = tree.Contains(TestString);

            // Assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public void Contains_SeveralStringsSomeInTarget_ReturnTrue()
        {
            // Arrange
            var keywords = new[] { "Windows", "Linux", "MacOSX" };
            var tree = new AhoCorasickTree(keywords);

            // Act
            var result = tree.Contains(TestString);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_SeveralKvalsNotFoundSomeInTarget_ReturnFalse()
        {
            // Arrange
            var keywords = new[]
            {
                new KeyValuePair<string, int>("Google", 1),
                new KeyValuePair<string, int>("Linux", 2),
                new KeyValuePair<string, int>("MacOSX", 3)
            };
            var tree = new AhoCorasickTree<int>(keywords);

            // Act
            var result = tree.Contains(TestString);

            // Assert
            result.Should().BeFalse();
        }
        
        [Fact]
        public void Contains_SeveralStringNotFoundSomeInTarget_ReturnFalse()
        {
            // Arrange
            var keywords = new[] { "Google", "Linux", "MacOSX" };
            var tree = new AhoCorasickTree(keywords);

            // Act
            var result = tree.Contains(TestString);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ContainsThatStart_SeveralKvalsSomeInTargetButStringDoesntStartsWithAny_ReturnFalse()
        {
            // Arrange
            var keywords = new[]
            {
                new KeyValuePair<string, int>("Windows", 1),
                new KeyValuePair<string, int>("Linux", 2),
                new KeyValuePair<string, int>("MacOSX", 3)
            };
            var tree = new AhoCorasickTree<int>(keywords);

            // Act
            var result = tree.ContainsThatStart(TestString);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ContainsThatStart_SeveralStringsSomeInTargetButStringDoesntStartsWithAny_ReturnFalse()
        {
            // Arrange
            var keywords = new[] { "Windows", "Linux", "MacOSX" };
            var tree = new AhoCorasickTree(keywords);

            // Act
            var result = tree.ContainsThatStart(TestString);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ContainsThatStart_SeveralStringsSomeInTarget_ReturnTrue()
        {
            // Arrange
            var keywords = new[]
            {
                new KeyValuePair<string, int>("Mozilla", 1),
                new KeyValuePair<string, int>("Chrome", 2),
                new KeyValuePair<string, int>("IE", 3)
            };
            var tree = new AhoCorasickTree<int>(keywords);

            // Act
            var result = tree.ContainsThatStart(TestString);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void FindAll_SeveralStringsSomeInTarget_ReturnAllFound()
        {
            // Arrange
            var keywords = new[] { "Windows", "Linux", "MacOSX" };
            var tree = new AhoCorasickTree(keywords);

            // Act
            var result = tree.FindAll(TestString);

            // Assert
            result.Should().BeEquivalentTo("Windows");
        }
        
        [Fact]
        public void FindAllValues_SeveralKvalsSomeInTarget_ReturnAllFound()
        {
            // Arrange
            var keywords = new[]
            {
                new KeyValuePair<string, int>("Windows", 1),
                new KeyValuePair<string, int>("Linux", 2),
                new KeyValuePair<string, int>("MacOSX", 3)
            };
            var tree = new AhoCorasickTree<int>(keywords);

            // Act
            var result = tree.FindAllValues(TestString);

            // Assert
            result.Should().BeEquivalentTo(1);
        }

        [Fact]
        public void FindAllValues_SeveralStringsSomeInTargetFoundMoreThenOne_ReturnAllFound()
        {
            // Arrange
            var keywords = new[]
            {
                new KeyValuePair<string, int>("Mozilla", 1),
                new KeyValuePair<string, int>("Chrome", 2),
                new KeyValuePair<string, int>("IE", 3)
            };
            var tree = new AhoCorasickTree<int>(keywords);

            // Act
            var result = tree.FindAllValues(TestString);

            // Assert
            result.Should().BeEquivalentTo(1, 2);
        }

        [Fact]
        public void FindAll_SeveralStringsSomeInTargetFoundMoreThenOne_ReturnAllFound()
        {
            // Arrange
            var keywords = new[] { "Mozilla", "Chrome", "IE" };
            var tree = new AhoCorasickTree(keywords);

            // Act
            var result = tree.FindAll(TestString);

            // Assert
            result.Should().BeEquivalentTo("Mozilla", "Chrome");
        }
        
        [Fact]
        public void FindAllValues_SeveralStringsWithSameKeySomeInTargetFoundMoreThenOne_ReturnAllFound()
        {
            // Arrange
            var keywords = new[]
            {
                new KeyValuePair<string, int>("Mozilla", 1),
                new KeyValuePair<string, int>("Mozilla", 4),
                new KeyValuePair<string, int>("Chrome", 2),
                new KeyValuePair<string, int>("IE", 3)
            };
            var tree = new AhoCorasickTree<int>(keywords);

            // Act
            var result = tree.FindAllValues(TestString);

            // Assert
            result.Should().BeEquivalentTo(1, 2, 4);
        }
        
        [Fact]
        public void FindAllValues_SeveralStringsWithSameKeySearchStringIsSubstringOfOneKey_NothingFound()
        {
            // Arrange
            var keywords = new[]
            {
                new KeyValuePair<string, int>("Mozilla", 1),
                new KeyValuePair<string, int>("Mozilla", 4),
                new KeyValuePair<string, int>("Chrome", 2),
                new KeyValuePair<string, int>("IE", 3)
            };
            var tree = new AhoCorasickTree<int>(keywords);

            // Act
            var result = tree.FindAllValues("Moz");

            // Assert
            result.Should().BeEmpty();
        }
        
        [Fact]
        public void FindAllNaive_SeveralStringsSomeInTargetFoundMoreThenOne_ReturnAllFound()
        {
            // Arrange
            var keywords = new[] { "Mozilla", "Chrome", "IE" };
            
            var sb = new StringBuilder(TestString);
            for (int i = 0; i < 100; i++)
            {
                sb.Append(TestString);
            }

            var bigString = sb.ToString();

            // Act
            int result = 0;
            for (int i = 0; i < 5000; i++)
            {
                result = FindAll(bigString, keywords).Count();
            }

            // Assert
            result.Should().BePositive();
        }
        
        private static IEnumerable<string> FindAll(string str, string[] keywords)
        {
            for (var i = 0; i < keywords.Length; i++)
            {
                for (int index = 0;; index += keywords[i].Length)
                {
                    index = str.IndexOf(keywords[i], index, StringComparison.CurrentCulture);
                    if (index == -1)
                        break;
                    yield return keywords[i];
                }
            }
        }
    }
}
