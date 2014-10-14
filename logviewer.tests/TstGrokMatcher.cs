// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Linq;
using logviewer.core;
using NUnit.Framework;

namespace logviewer.tests
{
    [TestFixture]
    public class TstGrokMatcher
    {
        [TestCase("%{ID}")]
        [TestCase("%{ID}%{DAT}")]
        [TestCase("%{ID} %{DAT}")]
        [TestCase("%{ID},%{DAT}")]
        [TestCase("%{ID}str%{DAT}")]
        [TestCase("str%{ID}str%{DAT}str")]
        [TestCase("%{ID}\"%{DAT}")]
        [TestCase("%{ID}'%{DAT}")]
        public void PositiveCompileTestsNotChangingString(string pattern)
        {
            this.PositiveCompileTestsThatChangeString(pattern, pattern);
        }

        [TestCase("%{WORD}", @"\b\w+\b")]
        [TestCase("%{ID}' %{} '%{DAT}", "%{ID} %{} %{DAT}")]
        [TestCase("%{ID}\" %{} \"%{DAT}", "%{ID} %{} %{DAT}")]
        [TestCase("%{ID}\"\\\" %{} \\\"\"%{DAT}", "%{ID}\" %{} \"%{DAT}")]
        [TestCase("%{ID}\"\\' %{} \\'\"%{DAT}", "%{ID}' %{} '%{DAT}")]
        [TestCase("%{ID}\"' %{} '\"%{DAT}", "%{ID}' %{} '%{DAT}")]
        [TestCase("%{ID}\"\\' \\\"%{}\\\" \\'\"%{DAT}", "%{ID}' \"%{}\" '%{DAT}")]
        [TestCase("%{ID}\"\\' \\'%{}\\' \\'\"%{DAT}", "%{ID}' '%{}' '%{DAT}")]
        [TestCase("%{ID}'\\\" %{} \\\"'%{DAT}", "%{ID}\" %{} \"%{DAT}")]
        [TestCase("%{ID}'\\' %{} \\''%{DAT}", "%{ID}' %{} '%{DAT}")]
        [TestCase("%{ID}'\" %{} \"'%{DAT}", "%{ID}\" %{} \"%{DAT}")]
        [TestCase("%{ID}\" %{} \"%{DAT}\" %{} \"", "%{ID} %{} %{DAT} %{} ")]
        [TestCase("\" %{} \"%{ID}\" %{} \"%{DAT}\" %{} \"", " %{} %{ID} %{} %{DAT} %{} ")]
        [TestCase("%{ID}''%{DAT}", "%{ID}%{DAT}")]
        [TestCase("%{ID}\"\"%{DAT}", "%{ID}%{DAT}")]
        [TestCase("%{WORD}%{ID}", @"\b\w+\b%{ID}")]
        [TestCase("%{WORD}%{INT}", @"\b\w+\b(?:[+-]?(?:[0-9]+))")]
        [TestCase("%{WORD} %{INT}", @"\b\w+\b (?:[+-]?(?:[0-9]+))")]
        [TestCase("%{WORD} %{INT} ", @"\b\w+\b (?:[+-]?(?:[0-9]+)) ")]
        [TestCase("%{WORD} %{INT}1234", @"\b\w+\b (?:[+-]?(?:[0-9]+))1234")]
        [TestCase("%{WORD} %{INT}str", @"\b\w+\b (?:[+-]?(?:[0-9]+))str")]
        [TestCase("%{WORD}str%{INT}trs", @"\b\w+\bstr(?:[+-]?(?:[0-9]+))trs")]
        [TestCase("%{TIME}", @"(?!<[0-9])(?:2[0123]|[01]?[0-9]):(?:[0-5][0-9])(?::(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))(?![0-9])")]
        [TestCase("%{TIMESTAMP_ISO8601}", @"(?>\d\d){1,2}-(?:0?[1-9]|1[0-2])-(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9])[T ](?:2[0123]|[01]?[0-9]):?(?:[0-5][0-9])(?::?(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))?(?:Z|[+-](?:2[0123]|[01]?[0-9])(?::?(?:[0-5][0-9])))?")]
        [TestCase("%{S1:s}%{S2:s}", @"%{S1}%{S2}")]
        [TestCase("%{LOGLEVEL:level}", @"(?<level>([A-a]lert|ALERT|[T|t]race|TRACE|[D|d]ebug|DEBUG|[N|n]otice|NOTICE|[I|i]nfo|INFO|[W|w]arn?(?:ing)?|WARN?(?:ING)?|[E|e]rr?(?:or)?|ERR?(?:OR)?|[C|c]rit?(?:ical)?|CRIT?(?:ICAL)?|[F|f]atal|FATAL|[S|s]evere|SEVERE|EMERG(?:ENCY)?|[Ee]merg(?:ency)?))")]
        [TestCase("%{POSINT:num,int}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [TestCase("%{POSINT:num,'0'->LogLevel.Trace,'1'->LogLevel.Debug,'2'->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [TestCase("%{POSINT:num,\"0\"->LogLevel.Trace,\"1\"->LogLevel.Debug,\"2\"->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [TestCase("%{POSINT:num,'  0 '->LogLevel.Trace,' 1 '->LogLevel.Debug,' 2'->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        public void PositiveCompileTestsThatChangeString(string pattern, string result)
        {
            var matcher = new GrokMatcher(pattern);
            Assert.That(matcher.CompilationFailed, Is.False);
            Assert.That(matcher.Template, Is.EqualTo(result));
        }

        [TestCase("%{POSINT:num,int,int}")]
        [TestCase("%{POSINT:num,int_number}")]
        [TestCase("%{POSINT:_num}")]
        [TestCase("%{POSINT:num1,num1}")]
        [TestCase("%{POSINT:N1}")]
        [TestCase("%{POSINT:1N}")]
        [TestCase("%{POSINT:1n}")]
        [TestCase("%{id}")]
        [TestCase("%{POSINT}%%{POSINT}")]
        [TestCase("%{POSINT}}%{POSINT}")]
        public void NegativeCompileTests(string pattern)
        {
            var matcher = new GrokMatcher(pattern);
            Assert.That(matcher.Template, Is.EqualTo(pattern));
            Assert.That(matcher.CompilationFailed, "Compilation must be failed but it wasn't");
        }

        [TestCase(",")]
        [TestCase(":")]
        [TestCase("->")]
        [TestCase(".")]
        [TestCase("_")]
        [TestCase(", ")]
        public void PositiveCompileExistWithSpecialChars(string special)
        {
            this.PositiveCompileTestsThatChangeString("%{WORD}" + special + "%{POSINT}", @"\b\w+\b" + special + @"\b(?:[1-9][0-9]*)\b");
        }

        [TestCase("%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}%{LOGLEVEL:level}%{DATA:head}", "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1")]
        [TestCase("%{MAC}", "00:15:F2:1E:D2:68")]
        [TestCase("^%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}", "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1")]
        public void PositiveMatch(string pattern, string message)
        {
            var matcher = new GrokMatcher(pattern);
            Assert.That(matcher.Match(message));
        }
        
        [Test]
        public void NegativeMatch()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601 %{DATA:meta}%{LOGLEVEL:level}%{DATA:head}");
            Assert.That(matcher.Match("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1"), Is.False);
        }

        [TestCase("%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}%{LOGLEVEL:level}%{DATA:head}")]
        [TestCase("%{TIMESTAMP_ISO8601:datetime,DateTime}%{DATA:meta}%{LOGLEVEL:level,LogLevel}%{DATA:head}")]
        public void ParseRealMessage(string pattern)
        {
            var matcher = new GrokMatcher(pattern);
            Assert.That(matcher.MessageSchema.Count, Is.EqualTo(4));
            Assert.That(matcher.CompilationFailed, Is.False);
            
            var result = matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1");
            Assert.That(result.ContainsKey("datetime"));
            Assert.That(result.ContainsKey("meta"));
            Assert.That(result.ContainsKey("level"));
            Assert.That(result.ContainsKey("head"));
        }

        [Test]
        public void ParseRealMessageNonDefaultCasting()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:datetime,DateTime}%{DATA}");
            Assert.That(matcher.MessageSchema.Count, Is.EqualTo(1));
            Assert.That(matcher.CompilationFailed, Is.False);
            
            var result = matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1");
            Assert.That(result.ContainsKey("datetime"));
            Assert.That(result.Keys.Count, Is.EqualTo(1));
            Assert.That(matcher.MessageSchema.First().CastingRules.Contains(new Rule("*")));
            var rule = new Rule("*");
            Assert.That(matcher.MessageSchema.First().Contains(rule));
            Assert.That(matcher.MessageSchema.First().CastingRules.First(r => r == rule).Type, Is.EqualTo("DateTime"));
        }
        
        [Test]
        public void ParseRealMessageWithDatatypesFailure()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:datetime,DateTime}%{DATA:meta}%{LOGLEVEL:level,LogLevel}%{DATA:head}");
            var result = matcher.Parse(" [4688] INFO \nmessage body 1");
            Assert.That(result, Is.Null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseSemanticWithTheSameName()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:dt}%{DATA:meta}%{LOGLEVEL:dt}%{DATA:head}");
            matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO Head");
        }
    }
}