// Created by: egr
// Created at: 14.08.2016
// © 2012-2016 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using logviewer.engine;
using logviewer.logic.storage;
using logviewer.logic.ui.statistic;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstStatisticModel : IDisposable
    {
        private const int DelayMilliseconds = 1000;
        private readonly Mock<ILogStore> store;
        private readonly StatisticModel model;
        private readonly string testPath = Guid.NewGuid().ToString();

        public TstStatisticModel()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            this.store = new Mock<ILogStore>();
            this.model = new StatisticModel(this.store.Object, "1", "ru");
        }

        [Fact]
        public void LoadStatistic_EmptyStore_AllLevelsCountZero()
        {
            // Arrange
            var stat = new Dictionary<LogLevel, long>
            {
                { LogLevel.Trace, 0 },
                { LogLevel.Debug, 0 },
                { LogLevel.Info, 0 },
                { LogLevel.Warn, 0 },
                { LogLevel.Error, 0 },
                { LogLevel.Fatal, 0 }
            };

            this.store.Setup(x => x.CountByLevel(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(stat.OrderBy(x => x.Key));

            this.store.Setup(
                x => x.SelectDateUsingFunc("min", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.MinValue);

            this.store.Setup(
                x => x.SelectDateUsingFunc("max", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.MinValue);

            this.store.SetupGet(x => x.DatabasePath).Returns(string.Empty);

            // Act
            this.model.LoadStatistic();

            // Assert
            SpinWait.SpinUntil(() => this.model.Items.Count == 10, TimeSpan.FromMilliseconds(DelayMilliseconds));
            this.model.Items.Single(x => x.Key == LogLevel.Trace.ToString()).Value.Should().Be("0");
            this.model.Items.Single(x => x.Key == LogLevel.Debug.ToString()).Value.Should().Be("0");
            this.model.Items.Single(x => x.Key == LogLevel.Info.ToString()).Value.Should().Be("0");
            this.model.Items.Single(x => x.Key == "Warning").Value.Should().Be("0");
            this.model.Items.Single(x => x.Key == LogLevel.Error.ToString()).Value.Should().Be("0");
            this.model.Items.Single(x => x.Key == LogLevel.Fatal.ToString()).Value.Should().Be("0");
        }

        [Theory]
        [InlineData(LogLevel.Trace, "Trace")]
        [InlineData(LogLevel.Debug, "Debug")]
        [InlineData(LogLevel.Info, "Info")]
        [InlineData(LogLevel.Warn, "Warning")]
        [InlineData(LogLevel.Error, "Error")]
        [InlineData(LogLevel.Fatal, "Fatal")]
        public void LoadStatistic_OnlyOneLevel_LevelReturnNotZero(LogLevel level, string name)
        {
            // Arrange
            var stat = new Dictionary<LogLevel, long>
            {
                { LogLevel.Trace, 0 },
                { LogLevel.Debug, 0 },
                { LogLevel.Info, 0 },
                { LogLevel.Warn, 0 },
                { LogLevel.Error, 0 },
                { LogLevel.Fatal, 0 }
            };
            stat[level] = 1;

            this.store.Setup(x => x.CountByLevel(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(stat.OrderBy(x => x.Key));

            this.store.Setup(
                x => x.SelectDateUsingFunc("min", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.MinValue);

            this.store.Setup(
                x => x.SelectDateUsingFunc("max", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.MinValue);

            this.store.SetupGet(x => x.DatabasePath).Returns(string.Empty);

            // Act
            this.model.LoadStatistic();

            // Assert
            SpinWait.SpinUntil(() => this.model.Items.Count == 10, TimeSpan.FromMilliseconds(DelayMilliseconds));
            this.model.Items.Single(x => x.Key == name).Value.Should().Be("1");
        }

        [Fact]
        public void LoadStatistic_DatesNotDefault_ItemsContainsDates()
        {
            // Arrange
            var stat = new Dictionary<LogLevel, long>
            {
                { LogLevel.Trace, 0 },
                { LogLevel.Debug, 0 },
                { LogLevel.Info, 0 },
                { LogLevel.Warn, 0 },
                { LogLevel.Error, 0 },
                { LogLevel.Fatal, 0 }
            };

            this.store.Setup(x => x.CountByLevel(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(stat.OrderBy(x => x.Key));

            this.store.Setup(
                x => x.SelectDateUsingFunc("min", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.Now);

            this.store.Setup(
                x => x.SelectDateUsingFunc("max", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.Now.AddYears(1));

            this.store.SetupGet(x => x.DatabasePath).Returns(string.Empty);

            // Act
            this.model.LoadStatistic();

            // Assert
            SpinWait.SpinUntil(() => this.model.Items.Count == 12, TimeSpan.FromMilliseconds(DelayMilliseconds));
            this.model.Items.Where(x => x.Key == "First").Should().HaveCount(1);
            this.model.Items.Where(x => x.Key == "Last").Should().HaveCount(1);
        }

        [Fact]
        public void LoadStatistic_AllItemsPresent_ItemsCountShouldBeAsSpecified()
        {
            // Arrange
            var stat = new Dictionary<LogLevel, long>
            {
                { LogLevel.Trace, 0 },
                { LogLevel.Debug, 0 },
                { LogLevel.Info, 0 },
                { LogLevel.Warn, 0 },
                { LogLevel.Error, 0 },
                { LogLevel.Fatal, 0 }
            };

            this.store.Setup(x => x.CountByLevel(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(stat.OrderBy(x => x.Key));

            this.store.Setup(
                x => x.SelectDateUsingFunc("min", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.Now);

            this.store.Setup(
                x => x.SelectDateUsingFunc("max", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.Now.AddYears(1));

            File.WriteAllText(this.testPath, "1");

            this.store.SetupGet(x => x.DatabasePath).Returns(this.testPath);

            // Act
            this.model.LoadStatistic();

            // Assert
            SpinWait.SpinUntil(() => this.model.Items.Count == 13, TimeSpan.FromMilliseconds(DelayMilliseconds)).Should().BeTrue();
        }

        public void Dispose()
        {
            if (File.Exists(this.testPath))
            {
                File.Delete(this.testPath);
            }
        }
    }
}