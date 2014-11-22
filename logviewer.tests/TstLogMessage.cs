// Created by: egr
// Created at: 02.01.2013
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using logviewer.engine;
using Xunit;
using Xunit.Extensions;

namespace logviewer.tests
{
    public class TstLogMessage
    {
        public TstLogMessage()
        {
            this.m = LogMessage.Create();
        }

        private LogMessage m;
        private const string H = "h";
        private const string B = "b";

        [Fact]
        public void Full()
        {
            this.m.AddLine(H);
            this.m.AddLine(B);
            Assert.False(this.m.IsEmpty);
            Assert.Equal(H, this.m.Header);
            Assert.Equal(B + "\n", this.m.Body);
        }

        [Fact]
        public void FullCached()
        {
            this.m.AddLine(H);
            this.m.AddLine(B);
            this.m.Cache(null);
            Assert.False(this.m.IsEmpty);
            Assert.Equal(H, this.m.Header);
            Assert.Equal(B, this.m.Body);
        }

        [Fact]
        public void IsEmpty()
        {
            Assert.True(this.m.IsEmpty);
            Assert.Empty(this.m.Body);
            Assert.Empty(this.m.Header);
        }
        
        [Fact]
        public void IsEmptyAllStringsEmpty()
        {
            this.m.AddLine(string.Empty);
            this.m.AddLine(string.Empty);
            Assert.False(this.m.IsEmpty);
            Assert.Empty(this.m.Header);
            Assert.Empty(this.m.Body);
            
        }

        [Fact]
        public void OnlyHead()
        {
            this.m.AddLine(H);
            Assert.False(this.m.IsEmpty);
            Assert.Equal(H, this.m.Header);
            Assert.Empty(this.m.Body);
        }

        [Fact]
        public void OnlyHeadCached()
        {
            this.m.AddLine(H);
            this.m.Cache(null);
            Assert.False(this.m.IsEmpty);
            Assert.Equal(H, this.m.Header);
            Assert.Empty(this.m.Body);
        }
        
        [Theory]
        [InlineData("Trace", LogLevel.Trace)]
        [InlineData("TRACE", LogLevel.Trace)]
        [InlineData("Debug", LogLevel.Debug)]
        [InlineData("DEBUG", LogLevel.Debug)]
        [InlineData("Info", LogLevel.Info)]
        [InlineData("Information", LogLevel.Trace)]
        [InlineData("INFO", LogLevel.Info)]
        [InlineData("Warn", LogLevel.Warn)]
        [InlineData("WARN", LogLevel.Warn)]
        [InlineData("Warning", LogLevel.Warn)]
        [InlineData("Error", LogLevel.Error)]
        [InlineData("ERROR", LogLevel.Error)]
        [InlineData("Fatal", LogLevel.Fatal)]
        [InlineData("FATAL", LogLevel.Fatal)]
        [InlineData("FaTaL", LogLevel.Fatal)]
        [InlineData("FaTa", LogLevel.Trace)]
        [InlineData("alert", LogLevel.Trace)]
        [InlineData("notice", LogLevel.Info)]
        [InlineData("severe", LogLevel.Fatal)]
        [InlineData("critical", LogLevel.Error)]
        [InlineData("emerg", LogLevel.Fatal)]
        [InlineData("emergency", LogLevel.Fatal)]
        public void ParseLogLevel(string input, LogLevel result)
        {
            this.ParseTest("level", "LogLevel", ParserType.LogLevel, input);
            Assert.Equal(result, (LogLevel)this.m.IntegerProperty("level"));
        }

        [Theory]
        [InlineData("2014-10-23 20:00:51,790", 2014, 10, 23, 20, 0, 51, 790)]
        [InlineData("2014-10-23 20:00:51.790", 2014, 10, 23, 20, 0, 51, 790)]
        [InlineData("2014-10-23 20:00:51", 2014, 10, 23, 20, 0, 51, 0)]
        public void ParseDateTime(string input, int y, int month, int d, int h, int min, int sec, int millisecond)
        {
            this.ParseTest("dt", "DateTime", ParserType.Datetime, input);
            Assert.Equal(new DateTime(y, month, d, h, min, sec, millisecond, DateTimeKind.Utc), DateTime.FromFileTimeUtc(this.m.IntegerProperty("dt")));
        }
        
        [Theory]
        [InlineData("24/Oct/2014:09:34:30 +0400", 2014, 10, 24, 9, 34, 30, 0, -4)]
        [InlineData("24/Oct/2014:09:34:30 +0000", 2014, 10, 24, 9, 34, 30, 0, 0)]
        public void ParseDateTimeTimezone(string input, int y, int month, int d, int h, int min, int sec, int millisecond, int offset)
        {
            this.ParseTest("dt", "DateTime", ParserType.Datetime, input);
            Assert.Equal(new DateTime(y, month, d, h + offset, min, sec, millisecond, DateTimeKind.Utc), DateTime.FromFileTimeUtc(this.m.IntegerProperty("dt")));
        }

        [Theory]
        [InlineData("1", 1, "long")]
        [InlineData("-1", -1, "long")]
        [InlineData("0", 0, "long")]
        [InlineData("a", 0, "long")]
        [InlineData("1", 1, "int")]
        [InlineData("1", 1, "Int32")]
        [InlineData("1", 1, "Int64")]
        [InlineData("9223372036854775807", long.MaxValue, "long")]
        [InlineData("9223372036854775807", long.MaxValue, "int")]
        [InlineData("9223372036854775807", long.MaxValue, "Int32")]
        [InlineData("9223372036854775807", long.MaxValue, "Int64")]
        [InlineData("-9223372036854775808", long.MinValue, "long")]
        [InlineData("-9223372036854775810", 0, "long")]
        [InlineData("9223372036854775810", 0, "long")]
        public void ParseInteger(string input, long result, string type)
        {
            this.ParseTest("integer", type, ParserType.Interger, input);
            Assert.Equal(result, this.m.IntegerProperty("integer"));
        }

        [Theory]
        [InlineData("s", "s", "string")]
        [InlineData("s", "s", "String")]
        public void ParseString(string input, string result, string type)
        {
            this.ParseTest("str", type, ParserType.String, input);
            Assert.Equal(result, this.m.StringProperty("str"));
        }

        [Theory]
        [InlineData("a", LogLevel.Debug)]
        [InlineData("100", LogLevel.Debug)]
        [InlineData("200", LogLevel.Info)]
        [InlineData("301", LogLevel.Warn)]
        [InlineData("404", LogLevel.Error)]
        [InlineData("410", LogLevel.Error)]
        [InlineData("416", LogLevel.Error)]
        [InlineData("500", LogLevel.Fatal)]
        public void ParseLogLevelCustomRules(string input, LogLevel result)
        {
            this.ParseTest("Level", ParserType.LogLevel, input, 
                new Rule("LogLevel.Info", "20"), 
                new Rule("LogLevel.Warn", "30"),
                new Rule("LogLevel.Error", "40"), 
                new Rule("LogLevel.Error", "41"), 
                new Rule("LogLevel.Fatal", "50"), 
                new Rule("LogLevel.Debug"));
            Assert.Equal(result, (LogLevel)this.m.IntegerProperty("Level"));
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