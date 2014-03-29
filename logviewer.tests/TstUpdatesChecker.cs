using System;
using System.Threading;
using System.Threading.Tasks;
using logviewer.core;
using NMock;
using NUnit.Framework;
using Is = NUnit.Framework.Is;

namespace logviewer.tests
{
    [TestFixture]
    public class TstUpdatesChecker
    {
        [SetUp]
        public void Setup()
        {
            this.mockery = new MockFactory();
            this.reader = this.mockery.CreateMock<IVersionsReader>();
            this.checker = new UpdatesChecker(this.reader.MockObject);
            this.invokeRead = this.reader.Expects.One.EventBinding<VersionEventArgs>(_ => _.VersionRead += null);
            this.invokeComplete = this.reader.Expects.One.EventBinding(_ => _.ReadCompleted += null);
            this.reader.Expects.One.Method(_ => _.ReadReleases());
            Task.Factory.StartNew(delegate
            {
                Thread.Sleep(100);
                this.invokeRead.Invoke(new VersionEventArgs(this.v1, string.Empty));
                this.invokeComplete.Invoke();
            });
        }

        private MockFactory mockery;
        private Mock<IVersionsReader> reader;
        private UpdatesChecker checker;
        private EventInvoker<VersionEventArgs> invokeRead;
        private EventInvoker invokeComplete;
        private readonly Version v1 = new Version(1, 2, 104, 0);

        [Test]
        public void Equal()
        {
            Assert.That(this.checker.IsUpdatesAvaliable(new Version(1, 2, 104, 0)), Is.False);
        }

        [Test]
        public void Greater()
        {
            Assert.That(this.checker.IsUpdatesAvaliable(new Version(2, 0)), Is.False);
        }

        [Test]
        public void Less()
        {
            Assert.That(this.checker.IsUpdatesAvaliable(new Version(1, 0)), Is.True);
        }
    }
}