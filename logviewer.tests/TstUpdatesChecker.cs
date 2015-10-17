// Created by: egr
// Created at: 29.03.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using logviewer.core;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstUpdatesChecker
    {
        public TstUpdatesChecker()
        {
            this.reader = new Mock<IVersionsReader>();
            this.checker = new UpdatesChecker(this.reader.Object);
            this.reader.Setup(_ => _.ReadReleases());
        }

        private void Invoke(params Version[] versions)
        {
            Task.Factory.StartNew(delegate
            {
                Thread.Sleep(30);
                foreach (var version in versions)
                {
                    reader.Raise(_ => _.VersionRead += null, new VersionEventArgs(version, string.Empty));
                }
                reader.Raise(_ => _.ReadCompleted += null);
            });
        }

        private readonly UpdatesChecker checker;
        private readonly Mock<IVersionsReader> reader;
        private static readonly Version v1 = new Version(1, 2, 104, 0);
        private static readonly Version v2 = new Version(1, 0);

        [Theory, MemberData("Versions")]
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

        [Theory, MemberData("Versions")]
        public void Less(Version[] versions)
        {
            this.Invoke(versions);
            Assert.True(this.checker.IsUpdatesAvaliable(v2));
        }

        public static IEnumerable<object[]> Versions => new []
        {
            new object[] { new [] { v1 } },
            new object[] { new [] { v2, v1 } },
            new object[] { new [] { v1, v2 } }

        };
    }
}