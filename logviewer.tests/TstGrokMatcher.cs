// Created by: egr
// Created at: 02.10.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Linq;
using logviewer.engine;
using Xunit;

namespace logviewer.tests
{
    public class TstGrokMatcher
    {
        [Fact]
        public void RuleWithTemplateAlternatives()
        {
            const string template = "%{MAC}";
            var matcher = new GrokMatcher(template);
            var fromtree = matcher.CreateTemplate();
            Assert.NotEqual(template, matcher.Template);
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
        public void PositiveCompileTestsNotChangingString(string pattern)
        {
            this.PositiveCompileTestsThatChangeString(pattern, pattern);
        }

        [Theory]
        [InlineData("%{WORD}", @"\b\w+\b")]
        [InlineData("%{ID}' %{} '%{DAT}", "%{ID} %{} %{DAT}")]
        [InlineData("%{ID}\" %{} \"%{DAT}", "%{ID} %{} %{DAT}")]
        [InlineData("%{ID}\"\\\" %{} \\\"\"%{DAT}", "%{ID}\" %{} \"%{DAT}")]
        [InlineData("%{ID}\"\\' %{} \\'\"%{DAT}", "%{ID}' %{} '%{DAT}")]
        [InlineData("%{ID}\"' %{} '\"%{DAT}", "%{ID}' %{} '%{DAT}")]
        [InlineData("%{ID}\"\\' \\\"%{}\\\" \\'\"%{DAT}", "%{ID}' \"%{}\" '%{DAT}")]
        [InlineData("%{ID}\"\\' \\'%{}\\' \\'\"%{DAT}", "%{ID}' '%{}' '%{DAT}")]
        [InlineData("%{ID}'\\\" %{} \\\"'%{DAT}", "%{ID}\" %{} \"%{DAT}")]
        [InlineData("%{ID}'\\' %{} \\''%{DAT}", "%{ID}' %{} '%{DAT}")]
        [InlineData("%{ID}'\" %{} \"'%{DAT}", "%{ID}\" %{} \"%{DAT}")]
        [InlineData("%{ID}\" %{} \"%{DAT}\" %{} \"", "%{ID} %{} %{DAT} %{} ")]
        [InlineData("\" %{} \"%{ID}\" %{} \"%{DAT}\" %{} \"", " %{} %{ID} %{} %{DAT} %{} ")]
        [InlineData("%{ID}''%{DAT}", "%{ID}%{DAT}")]
        [InlineData("%{ID}\"\"%{DAT}", "%{ID}%{DAT}")]
        [InlineData("%{WORD}%{ID}", @"\b\w+\b%{ID}")]
        [InlineData("%{WORD}%{INT}", @"\b\w+\b(?:[+-]?(?:[0-9]+))")]
        [InlineData("%{WORD} %{INT}", @"\b\w+\b (?:[+-]?(?:[0-9]+))")]
        [InlineData("%{WORD} %{INT} ", @"\b\w+\b (?:[+-]?(?:[0-9]+)) ")]
        [InlineData("%{WORD} %{INT}1234", @"\b\w+\b (?:[+-]?(?:[0-9]+))1234")]
        [InlineData("%{WORD} %{INT}str", @"\b\w+\b (?:[+-]?(?:[0-9]+))str")]
        [InlineData("%{WORD}str%{INT}trs", @"\b\w+\bstr(?:[+-]?(?:[0-9]+))trs")]
        [InlineData("%{TIME}", @"(?!<[0-9])(?:2[0123]|[01]?[0-9]):(?:[0-5][0-9])(?::(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))(?![0-9])")]
        [InlineData("%{TIMESTAMP_ISO8601}", @"(?>\d\d){1,2}-(?:0?[1-9]|1[0-2])-(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9])[T ](?:2[0123]|[01]?[0-9]):?(?:[0-5][0-9])(?::?(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))?(?:Z|[+-](?:2[0123]|[01]?[0-9])(?::?(?:[0-5][0-9])))?")]
        [InlineData("%{S1:s}%{S2:s}", @"%{S1}%{S2}")]
        [InlineData("%{LOGLEVEL:level}", @"(?<level>([A-a]lert|ALERT|[T|t]race|TRACE|[D|d]ebug|DEBUG|[N|n]otice|NOTICE|[I|i]nfo|INFO|[W|w]arn?(?:ing)?|WARN?(?:ING)?|[E|e]rr?(?:or)?|ERR?(?:OR)?|[C|c]rit?(?:ical)?|CRIT?(?:ICAL)?|[F|f]atal|FATAL|[S|s]evere|SEVERE|EMERG(?:ENCY)?|[Ee]merg(?:ency)?))")]
        [InlineData("%{POSINT:num,int}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{POSINT:num,'0'->LogLevel.Trace,'1'->LogLevel.Debug,'2'->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{POSINT:num,\"0\"->LogLevel.Trace,\"1\"->LogLevel.Debug,\"2\"->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{POSINT:num,'  0 '->LogLevel.Trace,' 1 '->LogLevel.Debug,' 2'->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{INT:Id,'0'->LogLevel.Trace}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'1'->LogLevel.Debug}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'2'->LogLevel.Info}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'3'->LogLevel.Warn}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'4'->LogLevel.Error}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'5'->LogLevel.Fatal}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'0'->LogLevel.Trace,'1'->LogLevel.Debug,'2'->LogLevel.Info,'3'->LogLevel.Warn,'4'->LogLevel.Error,'5'->LogLevel.Fatal}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:num_property}", "(?<num_property>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{POSINT}%%{POSINT}", "\\b(?:[1-9][0-9]*)\\b{POSINT}")]
        [InlineData("%{POSINT}}%{POSINT}", "\\b(?:[1-9][0-9]*)\\b}\\b(?:[1-9][0-9]*)\\b")]
        [InlineData("%{URIPATH}", @"(?:/[A-Za-z0-9$.+!*'(){},~:;=@#%_\-]*)+")]
        [InlineData("%{NGUSERNAME}", @"[a-zA-Z\.\@\-\+_%]+")]
        [InlineData("%{URIPARAM}", @"\?[A-Za-z0-9$.+!*'|(){},~@#%&/=:;_?\-\[\]]*")]
        public void PositiveCompileTestsThatChangeString(string pattern, string result)
        {
            var matcher = new GrokMatcher(pattern);
            var fromtree = matcher.CreateTemplate();
            Assert.False(matcher.CompilationFailed);
            Assert.Equal(result, matcher.Template);
        }

        [Theory]
        [InlineData("%{POSINT:num,int,int}")]
        [InlineData("%{POSINT:num,int_number}")]
        [InlineData("%{POSINT:_num}")]
        [InlineData("%{POSINT:num1,num1}")]
        [InlineData("%{POSINT:N1}")]
        [InlineData("%{POSINT:1N}")]
        [InlineData("%{POSINT:1n}")]
        [InlineData("%{id}")]
        [InlineData("%{POSINT:num,small}")]
        [InlineData("%{INT:Id,'0'->LogLevel.T}")]
        [InlineData("%{INT:Id,'0'->LogLevel.None}")]
        [InlineData("%{ID:Id,'0'->LogLevel.Trace}")]
        public void NegativeCompileTests(string pattern)
        {
            var matcher = new GrokMatcher(pattern);
            Assert.Equal(pattern, matcher.Template);
            Assert.True(matcher.CompilationFailed, "Compilation must be failed but it wasn't");
        }

        [Theory]
        [InlineData(",")]
        [InlineData(":")]
        [InlineData("->")]
        [InlineData(".")]
        [InlineData("_")]
        [InlineData(", ")]
        public void PositiveCompileExistWithSpecialChars(string special)
        {
            this.PositiveCompileTestsThatChangeString("%{WORD}" + special + "%{POSINT}", @"\b\w+\b" + special + @"\b(?:[1-9][0-9]*)\b");
        }

        [Theory]
        [InlineData("%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}%{LOGLEVEL:level}%{DATA:head}", "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1")]
        [InlineData("%{MAC}", "00:15:F2:1E:D2:68")]
        [InlineData("^%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}", "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1")]
        public void PositiveMatch(string pattern, string message)
        {
            var matcher = new GrokMatcher(pattern);
            Assert.True(matcher.Match(message));
        }
        
        [Fact]
        public void NegativeMatch()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601 %{DATA:meta}%{LOGLEVEL:level}%{DATA:head}");
            Assert.False(matcher.Match("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1"));
        }

        [Theory]
        [InlineData("%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}%{LOGLEVEL:level}%{DATA:head}")]
        [InlineData("%{TIMESTAMP_ISO8601:datetime,DateTime}%{DATA:meta}%{LOGLEVEL:level,LogLevel}%{DATA:head}")]
        public void ParseRealMessage(string pattern)
        {
            var matcher = new GrokMatcher(pattern);
            Assert.Equal(4, matcher.MessageSchema.Count);
            Assert.False(matcher.CompilationFailed);
            
            var result = matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1");
            Assert.True(result.ContainsKey("datetime"));
            Assert.True(result.ContainsKey("meta"));
            Assert.True(result.ContainsKey("level"));
            Assert.True(result.ContainsKey("head"));
        }

        [Theory]
        [InlineData("%{IIS}", 2)]
        [InlineData("%{COMMONAPACHELOG}", 10)]
        [InlineData("%{COMBINEDAPACHELOG}", 12)]
        [InlineData("%{SYSLOGPROG}", 2)]
        [InlineData("%{SYSLOGFACILITY}", 2)]
        [InlineData("%{SYSLOGBASE}", 6)]
        [InlineData("%{NGINXACCESS}", 16)]
        public void ParsePatternWithCastingInside(string pattern, int semanticCount)
        {
            var matcher = new GrokMatcher(pattern);
            var t = matcher.CreateTemplate();
            Assert.Equal(semanticCount, matcher.MessageSchema.Count);
            Assert.False(matcher.CompilationFailed);
            Assert.NotEqual(pattern, matcher.Template);
        }

        [Fact]
        public void ParseRealMessageNonDefaultCasting()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:datetime,DateTime}%{DATA}");
            Assert.Equal(1, matcher.MessageSchema.Count);
            Assert.False(matcher.CompilationFailed);
            
            var result = matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1");
            Assert.True(result.ContainsKey("datetime"));
            Assert.Equal(1, result.Keys.Count);
            Assert.True(matcher.MessageSchema.First().CastingRules.Contains(new Rule("*")));
            var rule = new Rule("*");
            Assert.True(matcher.MessageSchema.First().Contains(rule));
            Assert.Equal("DateTime", matcher.MessageSchema.First().CastingRules.First(r => r == rule).Type);
        }
        
        [Fact]
        public void ParseRealMessageWithDatatypesFailure()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:datetime,DateTime}%{DATA:meta}%{LOGLEVEL:level,LogLevel}%{DATA:head}");
            var result = matcher.Parse(" [4688] INFO \nmessage body 1");
            Assert.Null(result);
        }

        [Fact]
        public void ParseSemanticWithTheSameName()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:dt}%{DATA:meta}%{LOGLEVEL:dt}%{DATA:head}");
            Assert.Throws<ArgumentException>(delegate
            {
                matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO Head");
            });
        }
    }
}