// Created by: egr
// Created at: 02.01.2013
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using logviewer.core;
using NUnit.Framework;

namespace logviewer.tests
{
    [TestFixture]
    public class TstLogMessage
    {
        [SetUp]
        public void Setup()
        {
            this.m = LogMessage.Create();
        }

        private LogMessage m;
        private const string H = "h";
        private const string B = "b";

        [Test]
        public void Full()
        {
            this.m.AddLine(H);
            this.m.AddLine(B);
            Assert.That(this.m.IsEmpty, Is.False);
            Assert.That(this.m.Header, Is.EqualTo(H));
            Assert.That(this.m.Body, Is.EqualTo(B + "\n"));
        }

        [Test]
        public void FullCached()
        {
            this.m.AddLine(H);
            this.m.AddLine(B);
            this.m.Cache(null);
            Assert.That(this.m.IsEmpty, Is.False);
            Assert.That(this.m.Header, Is.EqualTo(H));
            Assert.That(this.m.Body, Is.EqualTo(B));
        }

        [Test]
        public void IsEmpty()
        {
            Assert.That(this.m.IsEmpty);
            Assert.That(this.m.Body, Is.Empty);
            Assert.That(this.m.Header, Is.Empty);
        }
        
        [Test]
        public void IsEmptyAllStringsEmpty()
        {
            this.m.AddLine(string.Empty);
            this.m.AddLine(string.Empty);
            Assert.That(this.m.IsEmpty, Is.False);
            Assert.That(this.m.Header, Is.Empty);
            Assert.That(this.m.Body, Is.Empty);
            
        }

        [Test]
        public void OnlyHead()
        {
            this.m.AddLine(H);
            Assert.That(this.m.IsEmpty, Is.False);
            Assert.That(this.m.Header, Is.EqualTo(H));
            Assert.That(this.m.Body, Is.Empty);
        }

        [Test]
        public void OnlyHeadCached()
        {
            this.m.AddLine(H);
            this.m.Cache(null);
            Assert.That(this.m.IsEmpty, Is.False);
            Assert.That(this.m.Header, Is.EqualTo(H));
            Assert.That(this.m.Body, Is.Empty);
        }
        
        [TestCase("Trace", LogLevel.Trace)]
        [TestCase("TRACE", LogLevel.Trace)]
        [TestCase("Debug", LogLevel.Debug)]
        [TestCase("DEBUG", LogLevel.Debug)]
        [TestCase("Info", LogLevel.Info)]
        [TestCase("Information", LogLevel.Trace)]
        [TestCase("INFO", LogLevel.Info)]
        [TestCase("Warn", LogLevel.Warn)]
        [TestCase("WARN", LogLevel.Warn)]
        [TestCase("Warning", LogLevel.Warn)]
        [TestCase("Error", LogLevel.Error)]
        [TestCase("ERROR", LogLevel.Error)]
        [TestCase("Fatal", LogLevel.Fatal)]
        [TestCase("FATAL", LogLevel.Fatal)]
        [TestCase("FaTaL", LogLevel.Fatal)]
        [TestCase("FaTa", LogLevel.Trace)]
        [TestCase("alert", LogLevel.Trace)]
        [TestCase("notice", LogLevel.Info)]
        [TestCase("severe", LogLevel.Fatal)]
        [TestCase("critical", LogLevel.Error)]
        [TestCase("emerg", LogLevel.Fatal)]
        [TestCase("emergency", LogLevel.Fatal)]
        public void ParseLogLevel(string input, LogLevel result)
        {
            this.ParseTest("level", "LogLevel", ParserType.LogLevel, input);
            Assert.That((LogLevel)this.m.IntegerProperty("level"), Is.EqualTo(result));
        }

        [TestCase("2014-10-23 20:00:51,790", 2014, 10, 23, 20, 0, 51, 790)]
        [TestCase("2014-10-23 20:00:51.790", 2014, 10, 23, 20, 0, 51, 790)]
        [TestCase("2014-10-23 20:00:51", 2014, 10, 23, 20, 0, 51, 0)]
        [TestCase("24/Oct/2014:09:34:30 +0400", 2014, 10, 24, 9, 34, 30, 0)]
        [TestCase("24/Oct/2014:09:34:30 +0000", 2014, 10, 24, 13, 34, 30, 0)]
        [TestCase("24/Oct/2014 09:34:30 +0400", 2014, 10, 24, 9, 34, 30, 0)]
        public void ParseDateTime(string input, int y, int month, int d, int h, int min, int sec, int millisecond)
        {
            this.ParseTest("dt", "DateTime", ParserType.Datetime, input);
            Assert.That(DateTime.FromFileTime(this.m.IntegerProperty("dt")), Is.EqualTo(new DateTime(y, month, d, h, min, sec, millisecond)));
        }

        [TestCase("1", 1, "long")]
        [TestCase("-1", -1, "long")]
        [TestCase("0", 0, "long")]
        [TestCase("a", 0, "long")]
        [TestCase("1", 1, "int")]
        [TestCase("1", 1, "Int32")]
        [TestCase("1", 1, "Int64")]
        [TestCase("9223372036854775807", long.MaxValue, "long")]
        [TestCase("9223372036854775807", long.MaxValue, "int")]
        [TestCase("9223372036854775807", long.MaxValue, "Int32")]
        [TestCase("9223372036854775807", long.MaxValue, "Int64")]
        [TestCase("-9223372036854775808", long.MinValue, "long")]
        [TestCase("-9223372036854775810", 0, "long")]
        [TestCase("9223372036854775810", 0, "long")]
        public void ParseInteger(string input, long result, string type)
        {
            this.ParseTest("integer", type, ParserType.Interger, input);
            Assert.That(this.m.IntegerProperty("integer"), Is.EqualTo(result));
        }

        [TestCase("s", "s", "string")]
        [TestCase("s", "s", "String")]
        public void ParseString(string input, string result, string type)
        {
            this.ParseTest("str", type, ParserType.String, input);
            Assert.That(this.m.StringProperty("str"), Is.EqualTo(result));
        }

        [TestCase("a", LogLevel.Debug)]
        [TestCase("100", LogLevel.Debug)]
        [TestCase("200", LogLevel.Info)]
        [TestCase("301", LogLevel.Warn)]
        [TestCase("404", LogLevel.Error)]
        [TestCase("410", LogLevel.Error)]
        [TestCase("416", LogLevel.Error)]
        [TestCase("500", LogLevel.Fatal)]
        public void ParseLogLevelCustomRules(string input, LogLevel result)
        {
            this.ParseTest("Level", ParserType.LogLevel, input, 
                new Rule("LogLevel.Info", "20"), 
                new Rule("LogLevel.Warn", "30"),
                new Rule("LogLevel.Error", "40"), 
                new Rule("LogLevel.Error", "41"), 
                new Rule("LogLevel.Fatal", "50"), 
                new Rule("LogLevel.Debug"));
            Assert.That((LogLevel)this.m.IntegerProperty("Level"), Is.EqualTo(result));
        }
        
        private void ParseTest(string prop, string type, ParserType parser, string input)
        {
            this.ParseTest(prop, parser, input, new Rule(type));
        }

        private void ParseTest(string prop, ParserType parser, string input, params Rule[] rules)
        {
            this.m.AddLine(H);
            IDictionary<SemanticProperty, ISet<Rule>> schema = new Dictionary<SemanticProperty, ISet<Rule>>
            {
                {
                    new SemanticProperty(prop, parser), new HashSet<Rule>(rules)
                }
            };
            IDictionary<string, string> props = new Dictionary<string, string>
            {
                {
                    prop, input
                }
            };
            this.m.AddProperties(props);
            this.m.Cache(schema);
        }
    }
}