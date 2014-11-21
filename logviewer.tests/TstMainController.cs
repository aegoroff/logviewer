// Created by: egr
// Created at: 19.09.2012
// © 2012-2014 Alexander Egorov

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using logviewer.core;
using logviewer.engine;
using Net.Sgoliver.NRtfTree.Util;
using NMock;
using NMock.Matchers;
using Xunit;
using Xunit.Extensions;

namespace logviewer.tests
{
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
            completed = false;
            CleanupTestFiles();
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            this.mockery = new MockFactory();
            this.view = this.mockery.CreateMock<ILogView>();
            this.settings = this.mockery.CreateMock<ISettingsProvider>();

            this.settings.Expects.One.GetProperty(_ => _.PageSize).WillReturn(100);
            this.settings.Expects.Any.Method(_ => _.FormatBody(new LogLevel())).WithAnyArguments().WillReturn(new RtfCharFormat());
            this.settings.Expects.Any.Method(_ => _.FormatHead(new LogLevel())).WithAnyArguments().WillReturn(new RtfCharFormat());

            var template = ParsingTemplate(core.ParsingTemplate.Defaults.First().StartMessage);
            this.settings.Expects.One.Method(_ => _.ReadParsingTemplate()).WillReturn(template);

            this.controller = new MainController(this.settings.MockObject);
            this.controller.ReadCompleted += this.OnReadCompleted;
            this.view.Expects.One.Method(_ => _.Initialize());
            this.view.Expects.One.SetProperty(_ => _.LogInfo).ToAnything();
            this.controller.SetView(this.view.MockObject);
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

        private static string FullPathToTestDb
        {
            get { return Path.Combine(SqliteSettingsProvider.ApplicationFolder, SettingsDb); }
        }

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
                throw new ExpectationException("Wait expired");
            }
        }

        #endregion

        private const string TestPath = "f";
        private const string RecentPath = "r";
        private readonly MockFactory mockery;
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
            this.WaitReadingComplete();
            Assert.Equal(1, this.controller.MessagesCount);
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
            Assert.Equal(count, this.controller.MessagesCount);
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
            Assert.Equal(c, this.controller.MessagesCount);
        }

        private bool completed;

        void OnReadCompleted(object sender, LogReadCompletedEventArgs e)
        {
            completed = true;
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
            Assert.Equal(c, this.controller.MessagesCount);
        }

        private void ReadLogExpectations()
        {
            this.view.Expects.One.Method(v => v.SetLoadedFileCapltion(null)).With(TestPath);
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.One.SetProperty(v => v.HumanReadableLogSize).ToAnything();
            this.view.Expects.AtLeastOne.SetProperty(v => v.LogInfo).ToAnything();
            this.view.Expects.AtLeast(2).Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.view.Expects.Exactly(2).Method(v => v.SetFileEncoding(null)).WithAnyArguments();
            this.view.Expects.AtLeastOne.Method(v => v.SetProgress(new LoadProgress())).WithAnyArguments();
        }

        [Fact]
        public void ExportRtfFail()
        {
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.One.MethodWith(v => v.OpenExport(TestPath + ".rtf")).WillReturn(false);
            this.controller.ExportToRtf();
        }

        [Fact]
        public void ExportRtfSuccess()
        {
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.One.MethodWith(v => v.OpenExport(TestPath + ".rtf")).WillReturn(true);
            this.view.Expects.One.Method(v => v.SaveRtf());
            this.controller.ExportToRtf();
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
            Assert.Equal(messages, this.controller.MessagesCount);
        }

        [Fact]
        public void OpenLogFileCanceled()
        {
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.view.Expects.One.Method(v => v.OpenLogFile()).WillReturn(false);
            this.controller.OpenLogFile();
        }

        [Fact]
        public void ReadEmptyFile()
        {
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.Create(TestPath).Dispose();
            Assert.Throws<ArgumentException>(() => this.controller.StartReadLog());
        }

        [Fact]
        public void ReadEmptyWhenMinGreaterThenMax()
        {
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.controller.MinFilter((int)LogLevel.Error);
            this.controller.MaxFilter((int)LogLevel.Info);
            File.Create(TestPath).Dispose();
            Assert.Throws<ArgumentException>(() => this.controller.StartReadLog());
        }

        [Fact]
        public void ReadFromBadPath()
        {
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.No.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.No.GetProperty(v => v.LogPath).WillReturn(string.Empty);
            Assert.Throws<FileNotFoundException>(() => this.controller.StartReadLog());
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
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.Exactly(2).Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.view.Expects.One.Method(v => v.SetLoadedFileCapltion(TestPath));
            File.WriteAllText(TestPath, sb.ToString());
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.Equal(2000, this.controller.MessagesCount);
        }

        void ReadNormalLogInternal(Encoding encoding)
        {
            this.ReadLogExpectations();

            File.WriteAllText(TestPath, MessageExamples, encoding);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.Equal(2, this.controller.MessagesCount);
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
            Assert.Equal(1, this.controller.MessagesCount);
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
            Assert.Equal(2, this.controller.MessagesCount);
        }

        [Fact]
        public void ReadRecentFilesEmpty()
        {
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.view.Expects.One.Method(v => v.ClearRecentFilesList());
            this.settings.Expects.Any.GetProperty(_ => _.KeepLastNFiles).WillReturn(KeepLastNFiles);
            this.settings.Expects.Any.GetProperty(_ => _.FullPathToDatabase).WillReturn(FullPathToTestDb);
            this.controller.ReadRecentFiles();
        }

        [Fact]
        public void SaveAndReadRecentFilesNoFile()
        {
            this.settings.Expects.Any.GetProperty(_ => _.KeepLastNFiles).WillReturn(KeepLastNFiles);
            this.settings.Expects.Any.GetProperty(_ => _.FullPathToDatabase).WillReturn(FullPathToTestDb);
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            using (this.mockery.Ordered())
            {
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(TestPath);
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.No.MethodWith(v => v.CreateRecentFileItem(TestPath));
            }
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.ReadRecentFiles();
        }

        [Fact]
        public void SaveAndReadRecentFiles()
        {
            CreateEmpty(f1, f2);
            this.settings.Expects.Any.GetProperty(_ => _.KeepLastNFiles).WillReturn(KeepLastNFiles);
            this.settings.Expects.Any.GetProperty(_ => _.FullPathToDatabase).WillReturn(FullPathToTestDb);
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            using (this.mockery.Ordered())
            {
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(f1);
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(f2);
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f2));
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f1));
            }
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.ReadRecentFiles();
        }
        
        [Fact]
        public void RecentFilesMoreThenLimit()
        {
            CreateEmpty(f1, f2, f3);
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.settings.Expects.Any.GetProperty(_ => _.KeepLastNFiles).WillReturn(KeepLastNFiles);
            this.settings.Expects.Any.GetProperty(_ => _.FullPathToDatabase).WillReturn(FullPathToTestDb);
            using (this.mockery.Ordered())
            {
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(f1);
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(f2);
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(f3);
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f3));
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f2));
            }
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.ReadRecentFiles();
        }
        
        [Fact]
        public void RecentFilesMoreThenLimitNoOneFile()
        {
            CreateEmpty(f1, f2);
            this.settings.Expects.Any.GetProperty(_ => _.KeepLastNFiles).WillReturn(KeepLastNFiles);
            this.settings.Expects.Any.GetProperty(_ => _.FullPathToDatabase).WillReturn(FullPathToTestDb);
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            using (this.mockery.Ordered())
            {
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(f1);
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(f2);
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn("file_3");
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f2));
            }
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.ReadRecentFiles();
        }

        [Fact]
        public void TotalPagesNoMessages()
        {
            // TODO: fix System.IO.IOException:
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            Assert.Equal(1, this.controller.TotalPages);
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
            Assert.Equal(pages, this.controller.TotalPages);
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
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            Assert.Equal(result, MainController.IsValidFilter(filter, useRegex));
        }

        [Fact]
        public void StartReadingWithinDelay()
        {
            this.settings.Expects.Exactly(2).SetProperty(_ => _.MessageFilter).To(new EqualMatcher("f"));
            this.settings.Expects.Any.GetProperty(_ => _.FullPathToDatabase).WillReturn(FullPathToTestDb);
            this.view.Expects.Any.Method(v => v.AddFilterItems(null)).WithAnyArguments();
            this.view.Expects.Any.Method(v => v.SetLoadedFileCapltion(null)).WithAnyArguments();
            this.view.Expects.Any.Method(v => v.StartReading());
            this.view.Expects.Any.Method(v => v.SetProgress(new LoadProgress())).WithAnyArguments();
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.view.Expects.No.Method(v => v.StartReading());
            this.controller.StartReading("f", false);
            this.controller.StartReading("f", false);
        }
        
        [Fact]
        public void StartReadingOutsideDelay()
        {
            // TODO: fix System.IO.IOException:
            this.settings.Expects.Exactly(2).SetProperty(_ => _.MessageFilter).To(new EqualMatcher("f"));
            this.settings.Expects.Any.GetProperty(_ => _.FullPathToDatabase).WillReturn(FullPathToTestDb);
            this.view.Expects.Any.Method(v => v.AddFilterItems(null)).WithAnyArguments();
            this.view.Expects.Any.Method(v => v.SetLoadedFileCapltion(null)).WithAnyArguments();
            this.view.Expects.Any.Method(v => v.StartReading());
            this.view.Expects.Any.Method(v => v.SetProgress(new LoadProgress())).WithAnyArguments();
            this.view.Expects.One.Method(v => v.SetLogProgressCustomText(null)).WithAnyArguments();
            this.view.Expects.One.Method(v => v.StartReading());
            this.controller.StartReading("f", false);
            Thread.Sleep(TimeSpan.FromMilliseconds(300));
            Assert.False(this.controller.PendingStart);
        }
    }
}