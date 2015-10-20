// Created by: egr
// Created at: 19.09.2012
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using logviewer.core;
using logviewer.engine;
using Moq;
using Net.Sgoliver.NRtfTree.Util;
using Xunit;
using Xunit.Sdk;

namespace logviewer.tests
{
    [Collection("SerialTests")]
    public class TstMainController : IDisposable
    {
        private const string f1 = "1";
        private const string f2 = "2";
        private const string f3 = "3";
        private const string SettingsDb = "test.db";
        private const int KeepLastNFiles = 2;

        #region Setup/Teardown

        public TstMainController()
        {
            this.completed = false;
            CleanupTestFiles();
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            this.view = new Mock<ILogView>();
            this.settings = new Mock<ISettingsProvider>();

            this.settings.SetupGet(_ => _.PageSize).Returns(100); // 1
            this.settings.Setup(_ => _.FormatBody(new LogLevel())).Returns(new RtfCharFormat()); // any
            this.settings.Setup(_ => _.FormatHead(new LogLevel())).Returns(new RtfCharFormat()); // any

            var template = ParsingTemplate(core.ParsingTemplate.Defaults.First().StartMessage);
            this.settings.Setup(_ => _.ReadParsingTemplate()).Returns(template); // 1

            this.controller = new MainController(this.settings.Object);
            this.controller.ReadCompleted += this.OnReadCompleted;
            this.view.Setup(_ => _.Initialize()); // 1
            this.view.SetupGet(_ => _.LogInfo).Returns(It.IsAny<string>()); // 1
            this.controller.SetView(this.view.Object);
        }

        private static ParsingTemplate ParsingTemplate(string startMessage)
        {
            return new ParsingTemplate
            {
                Index = 0,
                StartMessage = startMessage
            };
        }

        public void Dispose()
        {
            Cleanup(TestPath, RecentPath, FullPathToTestDb);
            CleanupTestFiles();
            this.controller.ReadCompleted -= this.OnReadCompleted;
            this.controller.Dispose();
        }

        private static string FullPathToTestDb => Path.Combine(SqliteSettingsProvider.ApplicationFolder, SettingsDb);

        private static void CleanupTestFiles()
        {
            Cleanup(f1, f2, f3);
        }
        
        private static void Cleanup(params string[] files)
        {
            foreach (var file in files.Where(File.Exists))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        private static void CreateEmpty(params string[] files)
        {
            foreach (var file in files)
            {
                using (File.Open(file, FileMode.Create))
                {
                }
            }
        }

        private void WaitReadingComplete()
        {
            var result = SpinWait.SpinUntil(() => this.completed, TimeSpan.FromSeconds(4));
            if (!result)
            {
                throw new XunitException("Wait expired");
            }
        }

        #endregion

        private const string TestPath = "f";
        private const string RecentPath = "r";
        private readonly Mock<ILogView> view;
        private readonly Mock<ISettingsProvider> settings;
        private readonly MainController controller;

        private const string MessageExamples =
            "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1\n2008-12-27 19:40:11,906 [5272] ERROR \nmessage body 2";

        internal static string MessageStart = core.ParsingTemplate.Defaults.First().StartMessage;

        [Fact]
        public void AllFilters()
        {
            this.ReadLogExpectations();
            
            File.WriteAllText(TestPath,
                MessageExamples + "\n2008-12-27 19:42:11,906 [5272] WARN \n2008-12-27 19:42:11,906 [5555] WARN ");
            this.controller.MinFilter((int)LogLevel.Warn);
            this.controller.MaxFilter((int)LogLevel.Warn);
            this.controller.TextFilter("5555");
            this.controller.UserRegexp(false);
            this.controller.StartReadLog();
            this.view.Verify();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(1);
        }

        [Theory]
        [InlineData((int)LogLevel.Warn, (int)LogLevel.Warn, MessageExamples + "\n2008-12-27 19:42:11,906 [5272] WARN ", 1)]
        [InlineData((int)LogLevel.Warn, (int)LogLevel.Warn, MessageExamples, 0)]
        public void MaxAndMaxFilter(int min, int max, string msg, int count)
        {
            this.ReadLogExpectations();
            
            File.WriteAllText(TestPath, msg);
            this.controller.MinFilter(min);
            this.controller.MaxFilter(max);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(count);
        }

        [Theory]
        [InlineData(LogLevel.Fatal, 2)]
        [InlineData(LogLevel.Error, 2)]
        [InlineData(LogLevel.Warn, 1)]
        [InlineData(LogLevel.Info, 1)]
        [InlineData(LogLevel.Debug, 0)]
        [InlineData(LogLevel.Trace, 0)]
        public void MaxFilter(LogLevel filter, int c)
        {
            File.WriteAllText(TestPath, MessageExamples);

            this.ReadLogExpectations();

            this.controller.MaxFilter((int)filter);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(c);
        }

        [Theory, MemberData("MaxDates")]
        public void MaxDate(DateTime filter, int c)
        {
            File.WriteAllText(TestPath, MessageExamples);

            this.ReadLogExpectations();

            this.controller.MaxDate(filter);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(c);
        }

        [Theory, MemberData("MinDates")]
        public void MinDate(DateTime filter, int c)
        {
            File.WriteAllText(TestPath, MessageExamples);

            this.ReadLogExpectations();

            this.controller.MinDate(filter);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(c);
        }

        public static IEnumerable<object[]> MaxDates => new[]
        {
            new object[] { new DateTime(2008, 12, 27, 19, 32, 0, DateTimeKind.Utc), 1 },
            new object[] { new DateTime(2008, 12, 27, 19, 52, 0, DateTimeKind.Utc), 2 },
            new object[] { new DateTime(2008, 12, 27, 19, 12, 0, DateTimeKind.Utc), 0 }
        };

        public static IEnumerable<object[]> MinDates => new[]
        {
            new object[] { new DateTime(2008, 12, 27, 19, 32, 0, DateTimeKind.Utc), 1 },
            new object[] { new DateTime(2008, 12, 27, 19, 52, 0, DateTimeKind.Utc), 0 },
            new object[] { new DateTime(2008, 12, 27, 19, 12, 0, DateTimeKind.Utc), 2 }
        };

        private bool completed;

        void OnReadCompleted(object sender, LogReadCompletedEventArgs e)
        {
            this.completed = true;
        }

        [Theory]
        [InlineData(LogLevel.Fatal, 0)]
        [InlineData(LogLevel.Error, 1)]
        [InlineData(LogLevel.Warn, 1)]
        [InlineData(LogLevel.Info, 2)]
        [InlineData(LogLevel.Debug, 2)]
        [InlineData(LogLevel.Trace, 2)]
        public void MinFilter(LogLevel filter, int c)
        {
            File.WriteAllText(TestPath, MessageExamples);

            this.ReadLogExpectations();

            this.controller.MinFilter((int)filter);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(c);
        }

        private void ReadLogExpectations()
        {
            this.view.Setup(v => v.SetLoadedFileCapltion(TestPath)); // 1
            this.view.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            this.view.SetupSet(v => v.LogPath = It.IsAny<string>()); // 1
            this.view.SetupSet(v => v.LogInfo = It.IsAny<string>()); // 1+
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 2+

            this.view.Setup(v => v.SetFileEncoding(It.IsAny<string>())); // 2
            this.view.Setup(v => v.SetProgress(It.IsAny<LoadProgress>())); // 1+
            this.settings.Setup(_=>_.FormatHead(It.IsAny<LogLevel>())).Returns(new RtfCharFormat { Font = "Courier New" });
            this.settings.Setup(_=>_.FormatBody(It.IsAny<LogLevel>())).Returns(new RtfCharFormat { Font = "Courier New" });
        }

        [Fact]
        public void ExportRtfFail()
        {
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.view.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            this.view.Setup(v => v.OpenExport(TestPath + ".rtf")).Returns(false); // 1
            this.controller.ExportToRtf();
            this.view.Verify();
        }

        [Fact]
        public void ExportRtfSuccess()
        {
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.view.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            this.view.Setup(v => v.OpenExport(TestPath + ".rtf")).Returns(true); // 1
            this.view.Setup(v => v.SaveRtf()); // 1
            this.controller.ExportToRtf();
            this.view.Verify();
        }

        [Theory]
        [InlineData(".*5272.*", 1, true)]
        [InlineData(".*ERROR.*", 1, true)]
        [InlineData(".*message body 2.*", 1, true)]
        [InlineData("t[", 0, true)]
        [InlineData("message body 2", 1, false)]
        [InlineData("^(?!.*message body 2).*", 1, true)]
        public void FilterText(string filter, int messages, bool useRegexp)
        {
            this.ReadLogExpectations();

            File.WriteAllText(TestPath, MessageExamples);
            this.controller.TextFilter(filter);
            this.controller.UserRegexp(useRegexp);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(messages);
        }

        [Fact]
        public void OpenLogFileCanceled()
        {
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.view.Setup(v => v.OpenLogFile()).Returns(false); // 1
            this.controller.OpenLogFile();
        }

        [Fact]
        public void ReadEmptyFile()
        {
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.view.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            File.Create(TestPath).Dispose();
            Assert.Throws<ArgumentException>(() => this.controller.StartReadLog());
        }

        [Fact]
        public void ReadEmptyWhenMinGreaterThenMax()
        {
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1

            this.controller.MinFilter((int)LogLevel.Error);
            this.controller.MaxFilter((int)LogLevel.Info);
            File.Create(TestPath).Dispose();
            Assert.Throws<ArgumentException>(() => this.controller.StartReadLog());
        }

        [Fact]
        public void ReadFromBadPath()
        {
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1

            this.view.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            this.view.SetupGet(v => v.LogPath).Returns(string.Empty); // 0
            Assert.Throws<ArgumentException>(() => this.controller.StartReadLog());
        }

        [Fact]
        public void ReadNormalBigLog()
        {
            this.ReadLogExpectations();

            var sb = new StringBuilder();
            for (var i = 0; i < 1000; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            this.view.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 2
            this.view.Setup(v => v.SetLoadedFileCapltion(TestPath)); // 1
            File.WriteAllText(TestPath, sb.ToString());
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(2000);
        }

        void ReadNormalLogInternal(Encoding encoding)
        {
            this.ReadLogExpectations();

            File.WriteAllText(TestPath, MessageExamples, encoding);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(2);
        }

        [Fact]
        public void ReadNormalLog()
        {
            this.ReadNormalLogInternal(Encoding.UTF8);
        }

        [Fact]
        public void ReadNormalLogWin1251()
        {
            this.ReadNormalLogInternal(Encoding.GetEncoding("windows-1251"));
        }

        [Fact]
        public void ReadNotEmptyLogWithoutMessagesMatch()
        {
            this.ReadLogExpectations();

            File.WriteAllText(TestPath, "test log");
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(1);
        }
        
        [Theory]
        [InlineData(MessageExamples)]
        [InlineData("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1\n2008-12-27T19:40:11,906Z+03 [5272] ERROR \nmessage body 2")]
        public void LogLevelParsing(string examples)
        {
            this.ReadLogExpectations();

            File.WriteAllText(TestPath, examples);
            this.controller.MinFilter((int)LogLevel.Info);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.MessagesCount.Should().Be(2);
        }

        [Fact]
        public void ReadRecentFilesEmpty()
        {
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1

            this.view.Setup(v => v.ClearRecentFilesList()); // 1
            this.settings.SetupGet(v => v.KeepLastNFiles).Returns(KeepLastNFiles); // Any
            this.settings.SetupGet(v => v.FullPathToDatabase).Returns(FullPathToTestDb); // Any
            this.controller.ReadRecentFiles();
        }

        [Fact]
        public void SaveAndReadRecentFilesNoFile()
        {
            this.settings.SetupGet(v => v.KeepLastNFiles).Returns(KeepLastNFiles); // Any
            this.settings.SetupGet(v => v.FullPathToDatabase).Returns(FullPathToTestDb); // Any
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1

            this.view.SetupGet(v => v.LogPath).Returns(TestPath); // 1
            this.view.Setup(v => v.ClearRecentFilesList()); // 1
            this.view.Setup(v => v.CreateRecentFileItem(TestPath)); // 0

            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.ReadRecentFiles();
        }

        [Fact]
        public void SaveAndReadRecentFiles()
        {
            CreateEmpty(f1, f2);
            this.settings.SetupGet(v => v.KeepLastNFiles).Returns(KeepLastNFiles); // Any
            this.settings.SetupGet(v => v.FullPathToDatabase).Returns(FullPathToTestDb); // Any
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1

            this.view.SetupGet(v => v.LogPath).Returns(f1); // 1
            this.view.SetupGet(v => v.LogPath).Returns(f2); // 1
            this.view.Setup(v => v.ClearRecentFilesList()); // 1
            this.view.Setup(v => v.CreateRecentFileItem(f2)); // 1
            this.view.Setup(v => v.CreateRecentFileItem(f1)); // 1

            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.ReadRecentFiles();
        }
        
        [Fact]
        public void RecentFilesMoreThenLimit()
        {
            CreateEmpty(f1, f2, f3);
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1

            this.settings.SetupGet(v => v.KeepLastNFiles).Returns(KeepLastNFiles); // Any
            this.settings.SetupGet(v => v.FullPathToDatabase).Returns(FullPathToTestDb); // Any

            this.view.SetupGet(v => v.LogPath).Returns(f1); // 1
            this.view.SetupGet(v => v.LogPath).Returns(f2); // 1
            this.view.SetupGet(v => v.LogPath).Returns(f3); // 1
            this.view.Setup(v => v.ClearRecentFilesList());
            this.view.Setup(v => v.CreateRecentFileItem(f3)); // 1
            this.view.Setup(v => v.CreateRecentFileItem(f2)); // 1

            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.ReadRecentFiles();
        }
        
        [Fact]
        public void RecentFilesMoreThenLimitNoOneFile()
        {
            CreateEmpty(f1, f2);
            this.settings.SetupGet(v => v.KeepLastNFiles).Returns(KeepLastNFiles); // Any
            this.settings.SetupGet(v => v.FullPathToDatabase).Returns(FullPathToTestDb); // Any
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1

            this.view.SetupGet(v => v.LogPath).Returns(f1); // 1
            this.view.SetupGet(v => v.LogPath).Returns(f2); // 1
            this.view.SetupGet(v => v.LogPath).Returns("file_3"); // 1

            this.view.Setup(v => v.ClearRecentFilesList());
            this.view.Setup(v => v.CreateRecentFileItem(f2)); // 1

            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.ReadRecentFiles();
        }

        [Fact]
        public void TotalPagesNoMessages()
        {
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1

            this.controller.TotalPages.Should().Be(1);
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(60, 2)]
        [InlineData(100, 2)]
        public void PagingTests(int messages, int pages)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < messages; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            this.ReadLogExpectations();

            File.WriteAllText(TestPath, sb.ToString());
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            this.controller.TotalPages.Should().Be(pages);
        }

        [Theory]
        [InlineData("test", false, true)]
        [InlineData("t[", false, true)]
        [InlineData("t[", true, false)]
        [InlineData("t[1]", true, true)]
        [InlineData(null, true, true)]
        [InlineData(null, false, true)]
        public void FilterValidation(string filter, bool useRegex, bool result)
        {
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            MainController.IsValidFilter(filter, useRegex).Should().Be(result);
        }

        [Fact]
        public void StartReadingWithinDelay()
        {
            this.settings.SetupSet(_ => _.MessageFilter = "f"); // 2
            this.settings.SetupGet(_ => _.FullPathToDatabase).Returns(FullPathToTestDb); // any

            this.view.Setup(v => v.AddFilterItems(It.IsAny<string[]>())); // any
            this.view.Setup(v => v.SetLoadedFileCapltion(It.IsAny<string>()));  // any
            this.view.Setup(v => v.StartReading()); // Any
            this.view.Setup(v => v.SetProgress(It.IsAny<LoadProgress>())); // Any
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.view.Setup(v => v.StartReading()); // 0
            this.controller.StartReading("f", false);
            this.controller.StartReading("f", false);
        }
        
        [Fact]
        public void StartReadingOutsideDelay()
        {
            this.settings.SetupSet(_ => _.MessageFilter = "f"); // 2
            this.settings.SetupGet(_ => _.FullPathToDatabase).Returns(FullPathToTestDb); // any
            this.view.Setup(v => v.AddFilterItems(It.IsAny<string[]>())); // any
            this.view.Setup(v => v.SetLoadedFileCapltion(It.IsAny<string>()));  // any
            this.view.Setup(v => v.StartReading()); // Any
            this.view.Setup(v => v.SetProgress(It.IsAny<LoadProgress>())); // Any
            this.view.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.view.Setup(v => v.StartReading()); // 1
            this.controller.StartReading("f", false);
            Thread.Sleep(TimeSpan.FromMilliseconds(300));
            this.controller.PendingStart.Should().BeFalse();
        }
    }
}