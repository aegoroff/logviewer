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
        }

        private void Invoke(params Version[] versions)
        {
            Task.Factory.StartNew(delegate
            {
                Thread.Sleep(100);
                foreach (var version in versions)
                {
                    this.invokeRead.Invoke(new VersionEventArgs(version, string.Empty));
                }
                this.invokeComplete.Invoke();
            });
        }

        private MockFactory mockery;
        private Mock<IVersionsReader> reader;
        private UpdatesChecker checker;
        private EventInvoker<VersionEventArgs> invokeRead;
        private EventInvoker invokeComplete;
        private readonly Version v1 = new Version(1, 2, 104, 0);
        private readonly Version v2 = new Version(1, 0);

        [Test]
        public void EqualSingle()
        {
            this.Invoke(this.v1);
            Assert.That(this.checker.IsUpdatesAvaliable(new Version(1, 2, 104, 0)), Is.False);
            Assert.That(this.checker.LatestVersion, Is.EqualTo(this.v1));
        }
        
        [Test]
        public void EqualFirstLess()
        {
            this.Invoke(this.v2, this.v1);
            Assert.That(this.checker.IsUpdatesAvaliable(new Version(1, 2, 104, 0)), Is.False);
            Assert.That(this.checker.LatestVersion, Is.EqualTo(this.v1));
        }
        
        [Test]
        public void EqualLastLess()
        {
            this.Invoke(this.v1, this.v2);
            Assert.That(this.checker.IsUpdatesAvaliable(new Version(1, 2, 104, 0)), Is.False);
            Assert.That(this.checker.LatestVersion, Is.EqualTo(this.v1));
        }

        [Test]
        public void Greater()
        {
            var v = new Version(2, 0);
            this.Invoke(this.v1);
            Assert.That(this.checker.IsUpdatesAvaliable(v), Is.False);
            Assert.That(this.checker.LatestVersion, Is.EqualTo(v));
        }

        [Test]
        public void Less()
        {
            this.Invoke(this.v1);
            Assert.That(this.checker.IsUpdatesAvaliable(this.v2), Is.True);
        }
        
        [Test]
        public void LessFirstLess()
        {
            this.Invoke(this.v2, this.v1);
            Assert.That(this.checker.IsUpdatesAvaliable(this.v2), Is.True);
        }
        
        [Test]
        public void LessLastLess()
        {
            this.Invoke(this.v1, this.v2);
            Assert.That(this.checker.IsUpdatesAvaliable(this.v2), Is.True);
        }
    }
}