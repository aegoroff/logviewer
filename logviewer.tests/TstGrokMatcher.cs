﻿// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System.Text.RegularExpressions;
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
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID}"));
            Assert.That(matcher.Template, Is.EqualTo("%{ID}"));
        }

        [Test]
        public void MatchesSeveraUnexistlWithoutLiteral()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID}%{DATE}"));
            Assert.That(matcher.Template, Is.EqualTo("%{ID}%{DATE}"));
        }
        
        [Test]
        public void MatchesSeveralUnexist()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID} %{DATE}"));
            Assert.That(matcher.Template, Is.EqualTo("%{ID} %{DATE}"));
        }
        
        [Test]
        public void MatchesSeveralUnexistNotEmptyLiteral()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID},%{DATE}"));
            Assert.That(matcher.Template, Is.EqualTo("%{ID},%{DATE}"));
        }
        
        [Test]
        public void MatchesSeveralUnexistNotEmptyLiteralWithSeveralChars()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID}str%{DATE}"));
            Assert.That(matcher.Template, Is.EqualTo("%{ID}str%{DATE}"));
        }
        
        [Test]
        public void NotMatches()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{id}"), Is.False);
            Assert.That(matcher.Template, Is.Null);
        }

        [Test]
        public void MatchesExist()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD}"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b"));
        }
        
        [Test]
        public void MatchesExistWithSemantic()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{LOGLEVEL:level}"));
            Assert.That(matcher.Template, Is.EqualTo(@"([A-a]lert|ALERT|[T|t]race|TRACE|[D|d]ebug|DEBUG|[N|n]otice|NOTICE|[I|i]nfo|INFO|[W|w]arn?(?:ing)?|WARN?(?:ING)?|[E|e]rr?(?:or)?|ERR?(?:OR)?|[C|c]rit?(?:ical)?|CRIT?(?:ICAL)?|[F|f]atal|FATAL|[S|s]evere|SEVERE|EMERG(?:ENCY)?|[Ee]merg(?:ency)?)"));
        }

        [Test]
        public void MatchesExistWithSemanticAndCasting()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{POSINT:num:int}"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b(?:[1-9][0-9]*)\b"));
        }
        
        [Test]
        public void MatchesExistAndUnexist()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD}%{ID}"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b%{ID}"));
        }

        [Test]
        public void MatchesSeveralExistWithoutLiteral()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD}%{INT}"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b(?:[+-]?(?:[0-9]+))"));
        }
        
        [Test]
        public void MatchesSeveralExist()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD} %{INT}"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b (?:[+-]?(?:[0-9]+))"));
        }
        
        [Test]
        public void MatchesSeveralExistLiteralEnd()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD} %{INT} "));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b (?:[+-]?(?:[0-9]+)) "));
        }
        
        [Test]
        public void MatchesSeveralExistLiteralEndManyChars()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD} %{INT}str"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b (?:[+-]?(?:[0-9]+))str"));
        }
        
        [Test]
        public void MatchesSeveralExistNotEmptyLiteral()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD}, %{INT}"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b, (?:[+-]?(?:[0-9]+))"));
        }
        
        [Test]
        public void MatchesSeveralExistNotEmptyLiteralWithoutSpace()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD}str%{INT}"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\bstr(?:[+-]?(?:[0-9]+))"));
        }
        
        [Test]
        public void MatchesSeveralExistNotEmptyLiteralWithoutSpaceWithTrail()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD}str%{INT}trs"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\bstr(?:[+-]?(?:[0-9]+))trs"));
        }

        [Test]
        public void MatchesExistComplex()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{TIME}"));
            Assert.That(matcher.Template, Is.EqualTo(@"(?!<[0-9])(?:2[0123]|[01]?[0-9]):(?:[0-5][0-9])(?::(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))(?![0-9])"));
        }
        
        [Test]
        public void MatchesExistComplexSeveralLevels()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{TIMESTAMP_ISO8601}"));
            Assert.That(matcher.Template, Is.EqualTo(@"(?>\d\d){1,2}-(?:0?[1-9]|1[0-2])-(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9])[T ](?:2[0123]|[01]?[0-9]):?(?:[0-5][0-9])(?::?(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))?(?:Z|[+-](?:2[0123]|[01]?[0-9])(?::?(?:[0-5][0-9])))?"));
        }
        
        [Test]
        public void CheckRealMessage()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}%{LOGLEVEL:level}%{DATA:head}"));
            const string m = "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1";
            Regex r = new Regex(matcher.Template);
            Assert.That(r.IsMatch(m));
        }
    }
}