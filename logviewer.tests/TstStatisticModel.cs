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

        [Fact]
        public void LoadStatistic_EmptyStore_AllLevelsCountZero()
        {
            // Arrange
            var store = new Mock<ILogStore>();
            StatisticModel model = new StatisticModel(store.Object, "1", "ru");

            store.Setup(
                x => x.CountMessages(It.IsAny<LogLevel>(), It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(0);

            store.Setup(
                x => x.SelectDateUsingFunc("min", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.MinValue);

            store.Setup(
                x => x.SelectDateUsingFunc("max", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.MinValue);

            store.SetupGet(x => x.DatabasePath).Returns(string.Empty);

            // Act
            model.LoadStatistic();

            // Assert
            SpinWait.SpinUntil(() => model.Items.Count == 10, TimeSpan.FromMilliseconds(DelayMilliseconds));
            model.Items.Single(x => x.Key == LogLevel.Trace.ToString()).Value.Should().Be("0");
            model.Items.Single(x => x.Key == LogLevel.Debug.ToString()).Value.Should().Be("0");
            model.Items.Single(x => x.Key == LogLevel.Info.ToString()).Value.Should().Be("0");
            model.Items.Single(x => x.Key == "Warning").Value.Should().Be("0");
            model.Items.Single(x => x.Key == LogLevel.Error.ToString()).Value.Should().Be("0");
            model.Items.Single(x => x.Key == LogLevel.Fatal.ToString()).Value.Should().Be("0");
        }

        [Fact]
        public void LoadStatistic_OnlyTrace_TraceReturnNotZero()
        {
            // Arrange
            var store = new Mock<ILogStore>();
            StatisticModel model = new StatisticModel(store.Object, "1", "ru");

            store.Setup(x => x.CountMessages(LogLevel.Trace, LogLevel.Trace, null, false, true)).Returns(1);
            store.Setup(x => x.CountMessages(LogLevel.Debug, LogLevel.Debug, null, false, true)).Returns(0);
            store.Setup(x => x.CountMessages(LogLevel.Info, LogLevel.Info, null, false, true)).Returns(0);
            store.Setup(x => x.CountMessages(LogLevel.Warn, LogLevel.Warn, null, false, true)).Returns(0);
            store.Setup(x => x.CountMessages(LogLevel.Error, LogLevel.Error, null, false, true)).Returns(0);
            store.Setup(x => x.CountMessages(LogLevel.Fatal, LogLevel.Fatal, null, false, true)).Returns(0);

            store.Setup(
                x => x.SelectDateUsingFunc("min", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.MinValue);

            store.Setup(
                x => x.SelectDateUsingFunc("max", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.MinValue);

            store.SetupGet(x => x.DatabasePath).Returns(string.Empty);

            // Act
            model.LoadStatistic();

            // Assert
            SpinWait.SpinUntil(() => model.Items.Count == 10, TimeSpan.FromMilliseconds(DelayMilliseconds));
            model.Items.Single(x => x.Key == LogLevel.Trace.ToString()).Value.Should().Be("1");
        }

        [Fact]
        public void LoadStatistic_DatesNotDefault_ItemsContainsDates()
        {
            // Arrange
            var store = new Mock<ILogStore>();
            StatisticModel model = new StatisticModel(store.Object, "1", "ru");

            store.Setup(
                x => x.CountMessages(It.IsAny<LogLevel>(), It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(0);

            store.Setup(
                x => x.SelectDateUsingFunc("min", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.Now);

            store.Setup(
                x => x.SelectDateUsingFunc("max", LogLevel.Trace, LogLevel.Fatal, null, false)).Returns(DateTime.Now.AddYears(1));

            store.SetupGet(x => x.DatabasePath).Returns(string.Empty);

            // Act
            model.LoadStatistic();

            // Assert
            SpinWait.SpinUntil(() => model.Items.Count == 12, TimeSpan.FromMilliseconds(DelayMilliseconds));
            model.Items.Where(x => x.Key == "First").Should().HaveCount(1);
            model.Items.Where(x => x.Key == "Last").Should().HaveCount(1);
        }
    }
}