using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using logviewer.engine;
using Moq;
using TechTalk.SpecFlow;

namespace logviewer.tests
{
    [Binding]
    public class ReadLogSteps
    {
        private readonly Mock<ICharsetDetector> detector;
        private LogReader reader;
        private readonly MemoryStream stream;
        private List<LogMessage> result;

        public ReadLogSteps()
        {
            this.detector = new Mock<ICharsetDetector>();
            this.stream = new MemoryStream();
        }

        [Given(@"I have grok ""(.*)""")]
        public void GivenIHaveGrok(string grok)
        {
            var grokMatcher = new GrokMatcher(grok);
            this.reader = new LogReader(this.detector.Object, grokMatcher);
        }

        [Given(@"I have ""(.*)"" message in the stream")]
        public void GivenIHaveMessageInTheStream(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            this.stream.Write(buffer, 0, buffer.Length);
            this.stream.Seek(0, SeekOrigin.Begin);
        }

        [When(@"I start read log from stream")]
        public void WhenIStartReadLogFromStream()
        {
            this.result = this.reader.Read(this.stream, 0, Encoding.UTF8).ToList();
        }

        [Then(@"the read result should be ""(.*)""")]
        public void ThenTheReadResultShouldBe(string msg)
        {
            this.result[0].Header.Should().Be(msg);
        }

    }
}
