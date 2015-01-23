// Created by: egr
// Created at: 29.03.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using logviewer.core;
using NMock;
using Xunit;
using Xunit.Extensions;

namespace logviewer.tests
{
    public class TstUpdatesChecker
    {
        public TstUpdatesChecker()
        {
            var mockery = new MockFactory();
            var reader = mockery.CreateMock<IVersionsReader>();
            this.checker = new UpdatesChecker(reader.MockObject);
            this.invokeRead = reader.Expects.One.EventBinding<VersionEventArgs>(_ => _.VersionRead += null);
            this.invokeComplete = reader.Expects.One.EventBinding(_ => _.ReadCompleted += null);
            reader.Expects.One.Method(_ => _.ReadReleases());
        }

        private void Invoke(params Version[] versions)
        {
            Task.Factory.StartNew(delegate
            {
                Thread.Sleep(30);
                foreach (var version in versions)
                {
                    this.invokeRead.Invoke(new VersionEventArgs(version, string.Empty));
                }
                this.invokeComplete.Invoke();
            });
        }

        private readonly UpdatesChecker checker;
        private readonly EventInvoker<VersionEventArgs> invokeRead;
        private readonly EventInvoker invokeComplete;
        private static readonly Version v1 = new Version(1, 2, 104, 0);
        private static readonly Version v2 = new Version(1, 0);

        [Theory, PropertyData("Versions")]
        public void EqualLess(Version[] versions)
        {
            this.Invoke(versions);
            Assert.False(this.checker.IsUpdatesAvaliable(new Version(1, 2, 104, 0)));
            Assert.Equal(v1, this.checker.LatestVersion);
        }

        [Fact]
        public void Greater()
        {
            var v = new Version(2, 0);
            this.Invoke(v1);
            Assert.False(this.checker.IsUpdatesAvaliable(v));
            Assert.Equal(v, this.checker.LatestVersion);
        }

        [Theory, PropertyData("Versions")]
        public void Less(Version[] versions)
        {
            this.Invoke(versions);
            Assert.True(this.checker.IsUpdatesAvaliable(v2));
        }

        public static IEnumerable<object[]> Versions
        {
            get
            {
                return new []
                {
                    new object[] { new [] { v1 } },
                    new object[] { new [] { v2, v1 } },
                    new object[] { new [] { v1, v2 } }

                };
            }
        }
    }
}