// Created by: egr
// Created at: 02.01.2013
// © 2012-2014 Alexander Egorov

using System.Collections;
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
        [TestCase("INFO", LogLevel.Info)]
        [TestCase("Warn", LogLevel.Warn)]
        [TestCase("WARN", LogLevel.Warn)]
        [TestCase("Warning", LogLevel.Trace)]
        [TestCase("Error", LogLevel.Error)]
        [TestCase("ERROR", LogLevel.Error)]
        [TestCase("Fatal", LogLevel.Fatal)]
        [TestCase("FATAL", LogLevel.Fatal)]
        public void ParseLogLevel(string input, LogLevel result)
        {
            this.m.AddLine(H);
            SemanticProperty s = new SemanticProperty("level", ParserType.LogLevel);
            Rule r = new Rule("LogLevel");
            ISet<Rule> rules = new HashSet<Rule>();
            rules.Add(r);
            IDictionary<SemanticProperty, ISet<Rule>> schema = new Dictionary<SemanticProperty, ISet<Rule>>();
            schema.Add(s, rules);
            IDictionary<string, string> props = new Dictionary<string, string>();
            props.Add(s.Name, input);
            this.m.AddProperties(props);
            this.m.Cache(schema);

            Assert.That((LogLevel)this.m.IntegerProperty(s.Name), Is.EqualTo(result));
        }
    }
}