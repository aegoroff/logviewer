// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System;
using logviewer.core;
using NUnit.Framework;

namespace logviewer.tests
{
    [TestFixture]
    public class TstGrokMatcher
    {
        [Test]
        public void MatchesUnexist()
        {
            var matcher = new GrokMatcher("%{ID}");
            Assert.That(matcher.Template, Is.EqualTo("%{ID}"));
        }

        [Test]
        public void MatchesSeveraUnexistlWithoutLiteral()
        {
            var matcher = new GrokMatcher("%{ID}%{DATE}");
            Assert.That(matcher.Template, Is.EqualTo("%{ID}%{DATE}"));
        }
        
        [Test]
        public void MatchesSeveralUnexist()
        {
            var matcher = new GrokMatcher("%{ID} %{DATE}");
            Assert.That(matcher.Template, Is.EqualTo("%{ID} %{DATE}"));
        }
        
        [Test]
        public void MatchesSeveralUnexistNotEmptyLiteral()
        {
            var matcher = new GrokMatcher("%{ID},%{DATE}");
            Assert.That(matcher.Template, Is.EqualTo("%{ID},%{DATE}"));
        }
        
        [Test]
        public void MatchesSeveralUnexistNotEmptyLiteralWithSeveralChars()
        {
            var matcher = new GrokMatcher("%{ID}str%{DATE}");
            Assert.That(matcher.Template, Is.EqualTo("%{ID}str%{DATE}"));
        }
        
        [Test]
        public void MatchesSeveralLiteralsEverywhere()
        {
            var matcher = new GrokMatcher("str%{ID}str%{DATE}str");
            Assert.That(matcher.Template, Is.EqualTo("str%{ID}str%{DATE}str"));
        }
        
        [Test]
        public void NotMatches()
        {
            var matcher = new GrokMatcher("%{id}");
            Assert.That(matcher.Template, Is.EqualTo("%{id}"));
        }

        [Test]
        public void MatchesExist()
        {
            var matcher = new GrokMatcher("%{WORD}");
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b"));
        }

        [TestCase("%{POSINT:num,int,int}")]
        [TestCase("%{POSINT:num,int_number}")]
        [TestCase("%{POSINT:_num}")]
        [TestCase("%{POSINT:num1}")]
        [TestCase("%{POSINT:N1}")]
        [TestCase("%{POSINT:1N}")]
        [TestCase("%{POSINT:1n}")]
        public void NegativeMatchTests(string pattern)
        {
            var matcher = new GrokMatcher(pattern);
            Assert.That(matcher.Template, Is.EqualTo(pattern));
        }

        [TestCase(",")]
        [TestCase(":")]
        [TestCase("->")]
        [TestCase(".")]
        [TestCase("_")]
        [TestCase(", ")]
        public void MatchesExistWithSpecialChars(string special)
        {
            var matcher = new GrokMatcher("%{WORD}" + special + "%{POSINT}");
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b" + special + @"\b(?:[1-9][0-9]*)\b"));
        }
        
        [Test]
        public void MatchesExistWithSemantic()
        {
            var matcher = new GrokMatcher("%{LOGLEVEL:level}");
            Assert.That(matcher.Template, Is.EqualTo(@"(?<level>([A-a]lert|ALERT|[T|t]race|TRACE|[D|d]ebug|DEBUG|[N|n]otice|NOTICE|[I|i]nfo|INFO|[W|w]arn?(?:ing)?|WARN?(?:ING)?|[E|e]rr?(?:or)?|ERR?(?:OR)?|[C|c]rit?(?:ical)?|CRIT?(?:ICAL)?|[F|f]atal|FATAL|[S|s]evere|SEVERE|EMERG(?:ENCY)?|[Ee]merg(?:ency)?))"));
        }

        [Test]
        public void MatchesExistWithSemanticAndCasting()
        {
            var matcher = new GrokMatcher("%{POSINT:num,int}");
            Assert.That(matcher.Template, Is.EqualTo(@"(?<num>\b(?:[1-9][0-9]*)\b)"));
        }

        [Test]
        public void MatchesExistWithSemanticAndComplexCasting()
        {
            var matcher = new GrokMatcher("%{POSINT:num,'0'->LogLevel.Trace,'1'->LogLevel.Debug,'2'->LogLevel.Info}");
            Assert.That(matcher.Template, Is.EqualTo(@"(?<num>\b(?:[1-9][0-9]*)\b)"));
        }
        
        [Test]
        public void MatchesExistWithSemanticAndComplexCastingStringWithDoubleQuotes()
        {
            var matcher = new GrokMatcher("%{POSINT:num,\"0\"->LogLevel.Trace,\"1\"->LogLevel.Debug,\"2\"->LogLevel.Info}");
            Assert.That(matcher.Template, Is.EqualTo(@"(?<num>\b(?:[1-9][0-9]*)\b)"));
        }
        
        [Test]
        public void MatchesExistWithSemanticAndComplexCastingWithSpacesInStrings()
        {
            var matcher = new GrokMatcher("%{POSINT:num,'  0 '->LogLevel.Trace,' 1 '->LogLevel.Debug,' 2'->LogLevel.Info}");
            Assert.That(matcher.Template, Is.EqualTo(@"(?<num>\b(?:[1-9][0-9]*)\b)"));
        }

        [Test]
        public void MatchesExistAndUnexist()
        {
            var matcher = new GrokMatcher("%{WORD}%{ID}");
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b%{ID}"));
        }

        [Test]
        public void MatchesSeveralExistWithoutLiteral()
        {
            var matcher = new GrokMatcher("%{WORD}%{INT}");
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b(?:[+-]?(?:[0-9]+))"));
        }
        
        [Test]
        public void MatchesSeveralExist()
        {
            var matcher = new GrokMatcher("%{WORD} %{INT}");
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b (?:[+-]?(?:[0-9]+))"));
        }
        
        [Test]
        public void MatchesSeveralExistLiteralEnd()
        {
            var matcher = new GrokMatcher("%{WORD} %{INT} ");
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b (?:[+-]?(?:[0-9]+)) "));
        }
        
        [Test]
        public void MatchesSeveralExistLiteralEndManyDigits()
        {
            var matcher = new GrokMatcher("%{WORD} %{INT}1234");
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b (?:[+-]?(?:[0-9]+))1234"));
        }
        
        [Test]
        public void MatchesSeveralExistLiteralEndManyChars()
        {
            var matcher = new GrokMatcher("%{WORD} %{INT}str");
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b (?:[+-]?(?:[0-9]+))str"));
        }
        
        [Test]
        public void MatchesSeveralExistNotEmptyLiteralWithoutSpace()
        {
            var matcher = new GrokMatcher("%{WORD}str%{INT}");
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\bstr(?:[+-]?(?:[0-9]+))"));
        }
        
        [Test]
        public void MatchesSeveralExistNotEmptyLiteralWithoutSpaceWithTrail()
        {
            var matcher = new GrokMatcher("%{WORD}str%{INT}trs");
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\bstr(?:[+-]?(?:[0-9]+))trs"));
        }

        [Test]
        public void MatchesExistComplex()
        {
            var matcher = new GrokMatcher("%{TIME}");
            Assert.That(matcher.Template, Is.EqualTo(@"(?!<[0-9])(?:2[0123]|[01]?[0-9]):(?:[0-5][0-9])(?::(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))(?![0-9])"));
        }
        
        [Test]
        public void MatchesExistComplexSeveralLevels()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601}");
            Assert.That(matcher.Template, Is.EqualTo(@"(?>\d\d){1,2}-(?:0?[1-9]|1[0-2])-(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9])[T ](?:2[0123]|[01]?[0-9]):?(?:[0-5][0-9])(?::?(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))?(?:Z|[+-](?:2[0123]|[01]?[0-9])(?::?(?:[0-5][0-9])))?"));
        }
        
        [Test]
        public void CheckRealMessage()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}%{LOGLEVEL:level}%{DATA:head}");
            Assert.That(matcher.Match("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1"));
        }
        
        [Test]
        public void CheckRealMessageSyntaxErrorInTemplate()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601 %{DATA:meta}%{LOGLEVEL:level}%{DATA:head}");
            Assert.That(matcher.Match("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1"), Is.False);
        }
        
        [Test]
        public void CompatibilityMatch()
        {
            var matcher = new GrokMatcher("^%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}");
            Assert.That(matcher.Match("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1"));
        }

        [TestCase("%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}%{LOGLEVEL:level}%{DATA:head}")]
        [TestCase("%{TIMESTAMP_ISO8601:datetime,DateTime}%{DATA:meta}%{LOGLEVEL:level,LogLevel}%{DATA:head}")]
        public void ParseRealMessage(string pattern)
        {
            var matcher = new GrokMatcher(pattern);
            var result = matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1");
            Assert.That(result.ContainsKey(new Semantic("datetime")));
            Assert.That(result.ContainsKey(new Semantic("meta")));
            Assert.That(result.ContainsKey(new Semantic("level")));
            Assert.That(result.ContainsKey(new Semantic("head")));
        }
        
        [Test]
        public void ParseRealMessageWithDatatypesFailure()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:datetime,DateTime}%{DATA:meta}%{LOGLEVEL:level,LogLevel}%{DATA:head}");
            var result = matcher.Parse(" [4688] INFO \nmessage body 1");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SemanticWithTheSameName()
        {
            var matcher = new GrokMatcher("%{S1:s}%{S2:s}");
            Assert.That(matcher.Template, Is.EqualTo(@"%{S1}%{S2}"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SemanticWithTheSameNameParse()
        {
            var matcher = new GrokMatcher("%{TIMESTAMP_ISO8601:dt}%{DATA:meta}%{LOGLEVEL:dt}%{DATA:head}");
            matcher.Parse("2008-12-27 19:31:47,250 [4688] INFO Head");
        }
    }
}