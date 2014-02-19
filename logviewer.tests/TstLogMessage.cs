// Created by: egr
// Created at: 02.01.2013
// © 2012-2014 Alexander Egorov

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
            Assert.That(this.m.Body, Is.EqualTo(B));
        }

        [Test]
        public void FullCached()
        {
            this.m.AddLine(H);
            this.m.AddLine(B);
            this.m.Cache();
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
            Assert.That(this.m.IsEmpty);
            Assert.That(this.m.Body, Is.Empty);
            Assert.That(this.m.Header, Is.Empty);
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
            this.m.Cache();
            Assert.That(this.m.IsEmpty, Is.False);
            Assert.That(this.m.Header, Is.EqualTo(H));
            Assert.That(this.m.Body, Is.Empty);
        }
    }
}