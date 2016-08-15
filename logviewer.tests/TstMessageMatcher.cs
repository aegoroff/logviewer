// Created by: egr
// Created at: 10.12.2015
// © 2012-2016 Alexander Egorov

using System.Text.RegularExpressions;
using FluentAssertions;
using logviewer.logic.models;
using Xunit;

namespace logviewer.tests
{
    public class TstMessageMatcher
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Ctor_IncludeAndExcludePatternsDefined_AllPropertiesNotNull(bool compiled)
        {
            // Arrange
            var template = new ParsingTemplate
            {
                StartMessage = "%{DATA}",
                Filter = "^//.*$",
                Compiled = compiled
            };

            // Act
            var matcher = new MessageMatcher(template, RegexOptions.ExplicitCapture);

            // Assert
            matcher.IncludeMatcher.Should().NotBeNull();
            matcher.ExcludeMatcher.Should().NotBeNull();
        }

        [Theory]
        [InlineData(false, "")]
        [InlineData(true, null)]
        [InlineData(true, " ")]
        [InlineData(true, "\n")]
        [InlineData(true, "\t")]
        public void Ctor_OnlyIncludePatternDefined_IncludeNotNullExcludeNull(bool compiled, string filter)
        {
            // Arrange
            var template = new ParsingTemplate
            {
                StartMessage = "%{DATA}",
                Filter = filter,
                Compiled = compiled
            };

            // Act
            var matcher = new MessageMatcher(template, RegexOptions.ExplicitCapture);

            // Assert
            matcher.IncludeMatcher.Should().NotBeNull();
            matcher.ExcludeMatcher.Should().BeNull();
        }

        [Fact]
        public void Ctor_IncludePatternNotDefined_PatternPropertiesNotSet()
        {
            // Arrange
            var template = new ParsingTemplate
            {
                Filter = "%{DATA}"
            };

            // Act
            var matcher = new MessageMatcher(template, RegexOptions.ExplicitCapture);

            // Assert
            matcher.IncludeMatcher.Should().BeNull();
            matcher.ExcludeMatcher.Should().BeNull();
        }
    }
}