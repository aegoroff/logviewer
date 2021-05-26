// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 29.03.2014
// © 2012-2018 Alexander Egorov

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
            var reader = new Mock<IVersionsReader>();
            this.observer = new Mock<IObserver<VersionModel>>();
            this.checker = new UpdatesChecker(reader.Object);
            reader.Setup(x => x.ReadReleases());
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
        private readonly Mock<IObserver<VersionModel>> observer;
        private static readonly Version v1 = new Version(1, 2, 104, 0);
        private static readonly Version v2 = new Version(1, 0);

        [Theory, MemberData(nameof(Versions))]
        public void EqualLess(Version[] versions)
        {
            this.Invoke(versions);
            this.checker.CheckUpdatesAvaliable(b => { }, new Version(1, 2, 104, 0));
            this.checker.LatestVersion.Should().Be(v1);
        }

        [Fact]
        public void Greater()
        {
            var v = new Version(2, 0);
            this.Invoke(v1);

            this.checker.CheckUpdatesAvaliable(b => { }, v);
            this.checker.LatestVersion.Should().Be(v);
        }

        [Theory, MemberData(nameof(Versions))]
        public void Less(Version[] versions)
        {
            this.Invoke(versions);
            this.checker.CheckUpdatesAvaliable(b => { }, v2);
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