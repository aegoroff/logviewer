// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 26.04.2017
// © 2012-2018 Alexander Egorov

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
        public void Contains_SeveralStringsNotFoundSomeInTarget_ReturnFalse()
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
            var keywords = new[] { "Mozilla", "Chrome", "IE" };
            var tree = new AhoCorasickTree(keywords);

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
    }
}
