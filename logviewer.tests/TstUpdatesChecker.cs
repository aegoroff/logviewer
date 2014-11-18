using System;
using System.Threading;
using System.Threading.Tasks;
using logviewer.core;
using NMock;
using Xunit;

namespace logviewer.tests
{
    public class TstUpdatesChecker
    {
        public TstUpdatesChecker()
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
        private readonly UpdatesChecker checker;
        private readonly EventInvoker<VersionEventArgs> invokeRead;
        private readonly EventInvoker invokeComplete;
        private readonly Version v1 = new Version(1, 2, 104, 0);
        private readonly Version v2 = new Version(1, 0);

        [Fact]
        public void EqualSingle()
        {
            this.Invoke(this.v1);
            Assert.False(this.checker.IsUpdatesAvaliable(new Version(1, 2, 104, 0)));
            Assert.Equal(this.v1, this.checker.LatestVersion);
        }
        
        [Fact]
        public void EqualFirstLess()
        {
            this.Invoke(this.v2, this.v1);
            Assert.False(this.checker.IsUpdatesAvaliable(new Version(1, 2, 104, 0)));
            Assert.Equal(this.v1, this.checker.LatestVersion);
        }
        
        [Fact]
        public void EqualLastLess()
        {
            this.Invoke(this.v1, this.v2);
            Assert.False(this.checker.IsUpdatesAvaliable(new Version(1, 2, 104, 0)));
            Assert.Equal(this.v1, this.checker.LatestVersion);
        }

        [Fact]
        public void Greater()
        {
            var v = new Version(2, 0);
            this.Invoke(this.v1);
            Assert.False(this.checker.IsUpdatesAvaliable(v));
            Assert.Equal(v, this.checker.LatestVersion);
        }

        [Fact]
        public void Less()
        {
            this.Invoke(this.v1);
            Assert.True(this.checker.IsUpdatesAvaliable(this.v2));
        }
        
        [Fact]
        public void LessFirstLess()
        {
            this.Invoke(this.v2, this.v1);
            Assert.True(this.checker.IsUpdatesAvaliable(this.v2));
        }
        
        [Fact]
        public void LessLastLess()
        {
            this.Invoke(this.v1, this.v2);
            Assert.True(this.checker.IsUpdatesAvaliable(this.v2));
        }
    }
}