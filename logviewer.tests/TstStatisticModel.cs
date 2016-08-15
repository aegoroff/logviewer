// Created by: egr
// Created at: 14.08.2016
// © 2012-2016 Alexander Egorov

using System;
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
    public class TstStatisticModel
    {
        private const int DelayMilliseconds = 1000;
        private readonly Mock<ILogStore> store;
        private readonly StatisticModel model;

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
            this.store.Setup(
                x => x.CountMessages(It.IsAny<LogLevel>(), It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(0);

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

        [Fact]
        public void LoadStatistic_OnlyTrace_TraceReturnNotZero()
        {
            // Arrange
            this.store.Setup(x => x.CountMessages(LogLevel.Trace, LogLevel.Trace, null, false, true)).Returns(1);
            this.store.Setup(x => x.CountMessages(LogLevel.Debug, LogLevel.Debug, null, false, true)).Returns(0);
            this.store.Setup(x => x.CountMessages(LogLevel.Info, LogLevel.Info, null, false, true)).Returns(0);
            this.store.Setup(x => x.CountMessages(LogLevel.Warn, LogLevel.Warn, null, false, true)).Returns(0);
            this.store.Setup(x => x.CountMessages(LogLevel.Error, LogLevel.Error, null, false, true)).Returns(0);
            this.store.Setup(x => x.CountMessages(LogLevel.Fatal, LogLevel.Fatal, null, false, true)).Returns(0);

            this.store.Setup(
                x => x.SelectDateUsingFunc("min", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.MinValue);

            this.store.Setup(
                x => x.SelectDateUsingFunc("max", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.MinValue);

            this.store.SetupGet(x => x.DatabasePath).Returns(string.Empty);

            // Act
            this.model.LoadStatistic();

            // Assert
            SpinWait.SpinUntil(() => this.model.Items.Count == 10, TimeSpan.FromMilliseconds(DelayMilliseconds));
            this.model.Items.Single(x => x.Key == LogLevel.Trace.ToString()).Value.Should().Be("1");
        }

        [Fact]
        public void LoadStatistic_DatesNotDefault_ItemsContainsDates()
        {
            // Arrange
            this.store.Setup(
                x => x.CountMessages(It.IsAny<LogLevel>(), It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(0);

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
    }
}