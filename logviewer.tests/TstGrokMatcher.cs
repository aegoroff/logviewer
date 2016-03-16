// Created by: egr
// Created at: 02.10.2014
// © 2012-2016 Alexander Egorov

using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using logviewer.engine;
using Xunit;
using Xunit.Abstractions;

namespace logviewer.tests
{
    public class TstGrokMatcher
    {
        private readonly ITestOutputHelper output;

        public TstGrokMatcher(ITestOutputHelper output)
        {
            this.output = output;
        }
        
        [Theory]
        [InlineData("%{ID}")]
        [InlineData("%{ID}%{DAT}")]
        [InlineData("%{ID} %{DAT}")]
        [InlineData("%{ID},%{DAT}")]
        [InlineData("%{ID}str%{DAT}")]
        [InlineData("str%{ID}str%{DAT}str")]
        [InlineData("%{ID}\"%{DAT}")]
        [InlineData("%{ID}'%{DAT}")]
        [InlineData("%{ID}\" %{DAT}")]
        [InlineData("%{ID}' %{DAT}")]
        [InlineData("%{ID} \"%{DAT}")]
        [InlineData("%{ID} '%{DAT}")]
        [InlineData("(?:(?:[A-Fa-f0-9]{2}-){5}[A-Fa-f0-9]{2})")]
        [InlineData("(?:(?:[A-Fa-f0-9]{2}:){5}[A-Fa-f0-9]{2})")]
        [InlineData("(?:(?:[A-Fa-f0-9]{4}\\.){2}[A-Fa-f0-9]{4})")]
        [InlineData("%{ID}' %{} '%{DAT}")]
        [InlineData("%{ID}\" %{} \"%{DAT}")]
        [InlineData("%{ID}\" %{} \"%{DAT}\" %{} \"")]
        [InlineData("\" %{} \"%{ID}\" %{} \"%{DAT}\" %{} \"")]
        [InlineData("%{ID}''%{DAT}")]
        [InlineData("%{ID}\"\"%{DAT}")]
        [InlineData("%{ID}'\" %{} \"'%{DAT}")]
        [InlineData("%{ID}'\\' %{} \\''%{DAT}")]
        [InlineData("%{ID}'\\\" %{} \\\"'%{DAT}")]
        [InlineData("%{ID}\"\\' \\'%{}\\' \\'\"%{DAT}")]
        [InlineData("%{ID}\"' %{} '\"%{DAT}")]
        [InlineData("%{ID}\"\\' \\\"%{}\\\" \\'\"%{DAT}")]
        [InlineData("%{ID}\"\\' %{} \\'\"%{DAT}")]
        [InlineData("%{ID}\"\\\" %{} \\\"\"%{DAT}")]
        public void Compile_NotChangingString_Success(string pattern)
        {
            // Act
            var matcher = new GrokMatcher(pattern, RegexOptions.None, this.output.WriteLine);

            // Assert
            matcher.CompilationFailed.Should().BeFalse();
            matcher.Template.Should().Be(pattern);
        }

        [Theory]
        [InlineData("%{WORD}", @"\b\w+\b")]
        [InlineData("%{WORD}%{ID}", @"\b\w+\b%{ID}")]
        [InlineData("%{WORD}%{INT}", @"\b\w+\b(?:[+-]?(?:[0-9]+))")]
        [InlineData("%{WORD} %{INT}", @"\b\w+\b (?:[+-]?(?:[0-9]+))")]
        [InlineData("%{WORD} %{INT} ", @"\b\w+\b (?:[+-]?(?:[0-9]+)) ")]
        [InlineData("%{WORD} %{INT}1234", @"\b\w+\b (?:[+-]?(?:[0-9]+))1234")]
        [InlineData("%{WORD} %{INT}str", @"\b\w+\b (?:[+-]?(?:[0-9]+))str")]
        [InlineData("%{WORD}str%{INT}trs", @"\b\w+\bstr(?:[+-]?(?:[0-9]+))trs")]
        [InlineData("%{TIME}", @"(?!<[0-9])(?:2[0123]|[01]?[0-9]):(?:[0-5][0-9])(?::(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))(?![0-9])")]
        [InlineData("%{TIMESTAMP_ISO8601}", @"(?>\d\d){1,2}-(?:0?[1-9]|1[0-2])-(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9])[T ](?:2[0123]|[01]?[0-9]):?(?:[0-5][0-9])(?::?(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))?(?:Z|[+-](?:2[0123]|[01]?[0-9])(?::?(?:[0-5][0-9])))?")]
        [InlineData("%{LOGLEVEL:level}", @"(?<level>([A-a]lert|ALERT|[T|t]race|TRACE|[D|d]ebug|DEBUG|[N|n]otice|NOTICE|[I|i]nfo|INFO|[W|w]arn?(?:ing)?|WARN?(?:ING)?|[E|e]rr?(?:or)?|ERR?(?:OR)?|[C|c]rit?(?:ical)?|CRIT?(?:ICAL)?|[F|f]atal|FATAL|[S|s]evere|SEVERE|EMERG(?:ENCY)?|[Ee]merg(?:ency)?))")]
        [InlineData("%{POSINT:num:int}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{POSINT:num:'0'->LogLevel.Trace,'1'->LogLevel.Debug,'2'->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{POSINT:num:\"0\"->LogLevel.Trace,\"1\"->LogLevel.Debug,\"2\"->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{POSINT:num:'  0 '->LogLevel.Trace,' 1 '->LogLevel.Debug,' 2'->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{INT:Id:'0'->LogLevel.Trace}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id:'1'->LogLevel.Debug}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id:'2'->LogLevel.Info}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id:'3'->LogLevel.Warn}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id:'4'->LogLevel.Error}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id:'5'->LogLevel.Fatal}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id:'0'->LogLevel.Trace,'1'->LogLevel.Debug,'2'->LogLevel.Info,'3'->LogLevel.Warn,'4'->LogLevel.Error,'5'->LogLevel.Fatal}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:num_property}", "(?<num_property>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{POSINT}%%{POSINT}", "\\b(?:[1-9][0-9]*)\\b%%{POSINT}")]
        [InlineData("%{POSINT}}%{POSINT}", "\\b(?:[1-9][0-9]*)\\b}\\b(?:[1-9][0-9]*)\\b")]
        [InlineData("%{URIPATH}", @"(?:/[A-Za-z0-9$.+!*'(){},~:;=@#%_\-]*)+")]
        [InlineData("%{NGUSERNAME}", @"[a-zA-Z\.\@\-\+_%]+")]
        [InlineData("%{URIPARAM}", @"\?[A-Za-z0-9$.+!*'|(){},~@#%&/=:;_?\-\[\]]*")]
        [InlineData("%{WORD:word:String}", @"(?<word>\b\w+\b)")]
        [InlineData("%{WORD:word:string}", @"(?<word>\b\w+\b)")]
        [InlineData("%{QUOTEDSTRING}", "(?>(?<!\\\\)(?>\"(?>\\\\.|[^\\\\\"]+)+\"|\"\"|(?>'(?>\\\\.|[^\\\\']+)+')|''|(?>`(?>\\\\.|[^\\\\`]+)+`)|``))")]
        [InlineData("%{USERNAME}", "[a-zA-Z0-9._-]+")]
        [InlineData("%{S1:s}%{S2:s}", "(?<s>%{S1})(?<s>%{S2})")]
        [InlineData("%{ID:Id:'0'->LogLevel.Trace}", "(?<Id>%{ID})")]
        [InlineData("%{WORD}'test'%{ID}", @"\b\w+\b'test'%{ID}")]
        public void Compile_ChangingString_Success(string pattern, string result)
        {
            // Act
            var matcher = new GrokMatcher(pattern, RegexOptions.None, this.output.WriteLine);

            // Assert
            matcher.CompilationFailed.Should().BeFalse();
            matcher.Template.Should().Be(result);
        }

        [Theory]
        [InlineData("%{POSINT:num:int:int}")]
        [InlineData("%{POSINT:num:int_number}")]
        [InlineData("%{POSINT:_num}")]
        [InlineData("%{POSINT:num1:num1}")]
        [InlineData("%{POSINT:N1}")]
        [InlineData("%{POSINT:1N}")]
        [InlineData("%{POSINT:1n}")]
        [InlineData("%{id}")]
        [InlineData("%{WORD")]
        [InlineData("%{POSINT:num:small}")]
        [InlineData("%{INT:Id:'0'->LogLevel.T}")]
        [InlineData("%{INT:Id:'0'->LogLevel.None}")]
        public void Compile_ErrorString_Failure(string pattern)
        {
            // Act
            var matcher = new GrokMatcher(pattern, RegexOptions.None, this.output.WriteLine);

            // Assert
            matcher.Template.Should().Be(pattern);
            matcher.CompilationFailed.Should().BeTrue("Compilation must be failed but it wasn't");
        }

        [Theory]
        [InlineData(",")]
        [InlineData(":")]
        [InlineData("->")]
        [InlineData(".")]
        [InlineData("_")]
        [InlineData(", ")]
        public void Compile_PatternWithReservedChars_Success(string special)
        {
            // Arrange
            var pattern = "%{WORD}" + special + "%{POSINT}";
            var result = @"\b\w+\b" + special + @"\b(?:[1-9][0-9]*)\b";

            // Act
            var matcher = new GrokMatcher(pattern, RegexOptions.None, this.output.WriteLine);

            // Assert
            matcher.CompilationFailed.Should().BeFalse();
            matcher.Template.Should().Be(result);
        }

        [Theory]
        [InlineData("%{IIS}", 2)]
        [InlineData("%{COMMONAPACHELOG}", 10)]
        [InlineData("%{COMBINEDAPACHELOG}", 12)]
        [InlineData("%{SYSLOGPROG}", 2)]
        [InlineData("%{SYSLOGFACILITY}", 2)]
        [InlineData("%{SYSLOGBASE}", 6)]
        [InlineData("%{NGINXACCESS}", 16)]
        [InlineData("%{COMMONAPACHELOG_LEVELED}", 2)]
        [InlineData("%{COMBINEDAPACHELOG_LEVELED}", 2)]
        public void Compile_PatternWithCastingInside_Success(string pattern, int semanticCount)
        {
            // Act
            var matcher = new GrokMatcher(pattern, RegexOptions.None, this.output.WriteLine);

            // Assert
            matcher.MessageSchema.Count.Should().Be(semanticCount);
            matcher.CompilationFailed.Should().BeFalse();
            matcher.Template.Should().NotBe(pattern);
        }

        [Theory]
        [InlineData("%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}%{LOGLEVEL:level}%{DATA:head}", "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1")]
        [InlineData("%{MAC}", "00:15:F2:1E:D2:68")]
        [InlineData("^%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}", "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1")]
        public void Match_ComplexPatterns_Success(string pattern, string message)
        {
            // Arrange
            var matcher = new GrokMatcher(pattern, RegexOptions.None, this.output.WriteLine);

            // Act
            var result = matcher.Match(message);

            // Assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public void Match_PatternWithSyntaxError_Failure()
        {
            // Arrange
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601 %{DATA:meta}%{LOGLEVEL:level}%{DATA:head}", RegexOptions.None, this.output.WriteLine);

            // Act
            var result = matcher.Match("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1");

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}%{LOGLEVEL:level}%{DATA:head}")]
        [InlineData("%{TIMESTAMP_ISO8601:datetime:DateTime}%{DATA:meta}%{LOGLEVEL:level:LogLevel}%{DATA:head}")]
        public void Parse_RealMessage_Success(string pattern)
        {
            // Arrange
            var matcher = new GrokMatcher(pattern, RegexOptions.None, this.output.WriteLine);

            // Act
            var result = matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1");

            // Assert
            matcher.MessageSchema.Count.Should().Be(4);
            matcher.CompilationFailed.Should().BeFalse();
            
            result.ContainsKey("datetime").Should().BeTrue();
            result.ContainsKey("meta").Should().BeTrue();
            result.ContainsKey("level").Should().BeTrue();
            result.ContainsKey("head").Should().BeTrue();
        }

        [Fact]
        public void Parse_RealMessageNonDefaultCasting_Success()
        {
            // Arrange
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:datetime:DateTime}%{DATA}", RegexOptions.None, this.output.WriteLine);
            
            // Act
            var result = matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1");

            // Assert
            Assert.Equal(1, matcher.MessageSchema.Count);
            Assert.False(matcher.CompilationFailed);

            result.ContainsKey("datetime").Should().BeTrue();
            result.Keys.Count.Should().Be(1);
            matcher.MessageSchema.First().CastingRules.Contains(new GrokRule(ParserType.Datetime)).Should().BeTrue();
            var rule = new GrokRule(ParserType.Datetime);
            matcher.MessageSchema.First().Contains(rule).Should().BeTrue();
            matcher.MessageSchema.First().CastingRules.First(r => r == rule).Type.Should().Be(ParserType.Datetime);
        }
        
        [Fact]
        public void Parse_RealMessageWithDatatypes_Failure()
        {
            // Arrange
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:datetime:DateTime}%{DATA:meta}%{LOGLEVEL:level:LogLevel}%{DATA:head}", RegexOptions.None, this.output.WriteLine);
            
            // Act
            var result = matcher.Parse(" [4688] INFO \nmessage body 1");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Parse_SemanticWithTheSameName_Throw()
        {
            // Arrange
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:dt}%{DATA:meta}%{LOGLEVEL:dt}%{DATA:head}", RegexOptions.None, this.output.WriteLine);

            // Act
            Assert.Throws<ArgumentException>(delegate
            {
                matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO Head");
            });
        }
    }
}