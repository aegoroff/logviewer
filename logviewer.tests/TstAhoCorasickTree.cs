using FluentAssertions;
using logviewer.engine;
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
        public void FindAll_SeveralStringsSomeInTarget_ReturnAllFound()
        {
            // Arrange
            var keywords = new[] { "Windows", "Linux", "MacOSX" };
            var tree = new AhoCorasickTree(keywords);

            // Act
            var result = tree.FindAll(TestString);

            // Assert
            result.ShouldBeEquivalentTo(new[] { "Windows" });
        }
    }
}
