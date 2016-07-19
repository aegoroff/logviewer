// Created by: egr
// Created at: 29.03.2014
// © 2012-2016 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using logviewer.logic;
using logviewer.logic.Annotations;
using logviewer.logic.models;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstUpdatesChecker
    {
        public TstUpdatesChecker()
        {
            this.reader = new Mock<IVersionsReader>();
            this.observer = new Mock<IObserver<VersionModel>>();
            this.checker = new UpdatesChecker(this.reader.Object);
        }

        private void Invoke(params Version[] versions)
        {
            Task.Factory.StartNew(delegate
            {
                Thread.Sleep(30);
                foreach (var version in versions)
                {
                    this.observer.Setup(o => o.OnNext(new VersionModel(version, string.Empty)));
                }
                this.observer.Setup(o => o.OnCompleted());
            });
        }

        private readonly UpdatesChecker checker;
        private readonly Mock<IVersionsReader> reader;
        private readonly Mock<IObserver<VersionModel>> observer;
        private static readonly Version v1 = new Version(1, 2, 104, 0);
        private static readonly Version v2 = new Version(1, 0);

        [Theory, MemberData(nameof(Versions))]
        public void EqualLess(Version[] versions)
        {
            this.Invoke(versions);
            this.checker.IsUpdatesAvaliable(new Version(1, 2, 104, 0)).Should().BeFalse();
            this.checker.LatestVersion.Should().Be(v1);
        }

        [Fact]
        public void Greater()
        {
            var v = new Version(2, 0);
            this.Invoke(v1);

            this.checker.IsUpdatesAvaliable(v).Should().BeFalse();
            this.checker.LatestVersion.Should().Be(v);
        }

        [Theory, MemberData(nameof(Versions))]
        public void Less(Version[] versions)
        {
            this.Invoke(versions);
            this.checker.IsUpdatesAvaliable(v2).Should().BeTrue();
        }

        [PublicAPI]
        public static IEnumerable<object[]> Versions => new []
        {
            new object[] { new [] { v1 } },
            new object[] { new [] { v2, v1 } },
            new object[] { new [] { v1, v2 } }
        };
    }
}