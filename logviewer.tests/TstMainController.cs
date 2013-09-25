// Created by: egr
// Created at: 19.09.2012
// © 2012-2013 Alexander Egorov

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using logviewer.core;
using NMock;
using NUnit.Framework;

namespace logviewer.tests
{
    [TestFixture]
    public class TstMainController
    {
        private const string f1 = "1";
        private const string f2 = "2";
        private const string f3 = "3";
        private const string SettingsDb = "test.db";
        private const int KeepLastNFiles = 2;

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            completed = false;
            CleanupTestFiles();
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            this.mockery = new MockFactory();
            this.view = this.mockery.CreateMock<ILogView>();
            this.controller = new MainController(MessageStart, this.levels, SettingsDb, KeepLastNFiles, 100);
            this.controller.ReadCompleted += this.OnReadCompleted;
            this.view.Expects.One.Method(_ => _.Initialize());
            this.controller.SetView(this.view.MockObject);
        }

        [TearDown]
        public void Teardown()
        {
            Cleanup(TestPath, RecentPath, Path.Combine(Settings.ApplicationFolder, SettingsDb));
            CleanupTestFiles();
            this.controller.Dispose();
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
            var result = SpinWait.SpinUntil(() => this.completed, TimeSpan.FromSeconds(2));
            if (!result)
            {
                throw new ExpectationException("Wait expired");
            }
        }

        #endregion

        private readonly string[] levels =
        {
            "TRACE",
            "DEBUG",
            "INFO",
            "WARN",
            "ERROR",
            "FATAL"
        };

        private const string TestPath = "f";
        private const string RecentPath = "r";
        private MockFactory mockery;
        private Mock<ILogView> view;
        private MainController controller;

        private const string MessageExamples =
            "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1\n2008-12-27 19:40:11,906 [5272] ERROR \nmessage body 2";

        private const string MessageStart = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}.*";

        [Test]
        public void AllFilters()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath,
                MessageExamples + "\n2008-12-27 19:42:11,906 [5272] WARN \n2008-12-27 19:42:11,906 [5555] WARN ");
            this.controller.MinFilter((int)LogLevel.Warn);
            this.controller.MaxFilter((int)LogLevel.Warn);
            this.controller.TextFilter("5555");
            this.controller.UserRegexp(false);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }

        [TestCase((int)LogLevel.Warn, (int)LogLevel.Warn, MessageExamples + "\n2008-12-27 19:42:11,906 [5272] WARN ", 1)]
        [TestCase((int)LogLevel.Warn, (int)LogLevel.Warn, MessageExamples, 0)]
        public void MaxAndMaxFilter(int min, int max, string msg, int count)
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, msg);
            this.controller.MinFilter(min);
            this.controller.MaxFilter(max);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(count));
        }

        [TestCase(LogLevel.Warn, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 1)]
        [TestCase(LogLevel.Info, new[] { "TRACE", "DEBUG", @"\[?INFO\]?", "WARN", "ERROR", "FATAL" }, 1)]
        [TestCase(LogLevel.Debug, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 0)]
        [TestCase(LogLevel.Error, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 2)]
        [TestCase(LogLevel.Trace, new[] { "TRACE", "DEBUG", @"\[?InFO\]?", "WARN", "ERROR", "FATAL" }, 1)]
        public void MaxFilter(LogLevel filter, string[] markers, int c)
        {
            File.WriteAllText(TestPath, MessageExamples);
            this.view.Expects.One.Method(_ => _.Initialize());
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            Cleanup(Path.Combine(Settings.ApplicationFolder, SettingsDb));
            this.controller = new MainController(MessageStart, markers, SettingsDb, KeepLastNFiles);
            this.controller.ReadCompleted += this.OnReadCompleted;
            this.controller.SetView(this.view.MockObject);
            this.controller.MaxFilter((int)filter);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(c));
        }

        private bool completed;

        void OnReadCompleted(object sender, LogReadCompletedEventArgs e)
        {
            completed = true;
        }

        [TestCase(LogLevel.Warn, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 1)]
        [TestCase(LogLevel.Error, new[] { "TRACE", "DEBUG", "INFO", "WARN", @"\[?ERROR\]?", "FATAL" }, 1)]
        [TestCase(LogLevel.Info, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 2)]
        [TestCase(LogLevel.Fatal, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 0)]
        [TestCase(LogLevel.Error, new[] { "TRACE", "DEBUG", "INFO", "WARN", @"\[?ERRoR\]?", "FATAL" }, 0)]
        public void MinFilter(LogLevel filter, string[] markers, int c)
        {
            File.WriteAllText(TestPath, MessageExamples);
            this.view.Expects.One.Method(_ => _.Initialize());
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            Cleanup(Path.Combine(Settings.ApplicationFolder, SettingsDb));
            this.controller = new MainController(MessageStart, markers, SettingsDb, KeepLastNFiles);
            this.controller.ReadCompleted += this.OnReadCompleted;
            this.controller.SetView(this.view.MockObject);
            this.controller.MinFilter((int)filter);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(c));
        }

        [Test]
        public void ExportRtfFail()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.One.MethodWith(v => v.OpenExport(TestPath + ".rtf")).WillReturn(false);
            this.controller.ExportToRtf();
        }

        [Test]
        public void ExportRtfSuccess()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.One.MethodWith(v => v.OpenExport(TestPath + ".rtf")).WillReturn(true);
            this.view.Expects.One.Method(v => v.SaveRtf());
            this.controller.ExportToRtf();
        }

        [TestCase(".*5272.*", 1, true)]
        [TestCase(".*ERROR.*", 1, true)]
        [TestCase(".*message body 2.*", 1, true)]
        [TestCase("t[", 0, true)]
        [TestCase("message body 2", 1, false)]
        [TestCase("^(?!.*message body 2).*", 1, true)]
        public void FilterText(string filter, int messages, bool useRegexp)
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples);
            this.controller.TextFilter(filter);
            this.controller.UserRegexp(useRegexp);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(messages));
        }

        [Test]
        public void OpenLogFileCanceled()
        {
            this.view.Expects.One.Method(v => v.OpenLogFile()).WillReturn(false);
            this.controller.OpenLogFile();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadEmptyFile()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.Create(TestPath).Dispose();
            this.controller.StartReadLog();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(0));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadEmptyWhenMinGreaterThenMax()
        {
            this.controller.MinFilter((int)LogLevel.Error);
            this.controller.MaxFilter((int)LogLevel.Info);
            File.Create(TestPath).Dispose();
            this.controller.StartReadLog();
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ReadFromBadPath()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.No.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.No.GetProperty(v => v.LogPath).WillReturn(string.Empty);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(0));
        }

        [Test]
        public void ReadNormalBigLog()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 1000; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, sb.ToString());
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2000));
        }

        [Test]
        public void ReadNormalLog()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples);
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2));
        }

        [Test]
        public void ReadNormalLogWin1251()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples, Encoding.GetEncoding("windows-1251"));
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2));
        }

        [Test]
        public void ReadNotEmptyLogWithoutMessagesMatch()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, "test log");
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void ReadRecentFilesEmpty()
        {
            this.view.Expects.One.Method(v => v.ClearRecentFilesList());
            this.controller.ReadRecentFiles();
        }

        [Test]
        public void SaveAndReadRecentFilesNoFile()
        {
            using (this.mockery.Ordered())
            {
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(TestPath);
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.No.MethodWith(v => v.CreateRecentFileItem(TestPath));
            }
            this.controller.AddCurrentFileToRecentFilesList();
            this.controller.ReadRecentFiles();
        }

        [Test]
        public void SaveAndReadRecentFiles()
        {
            CreateEmpty(f1, f2);
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
        
        [Test]
        public void RecentFilesMoreThenLimit()
        {
            CreateEmpty(f1, f2, f3);
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
        
        [Test]
        public void RecentFilesMoreThenLimitNoOneFile()
        {
            CreateEmpty(f1, f2);
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

        [Test]
        public void TotalPagesNoMessages()
        {
            Assert.That(this.controller.TotalPages, NUnit.Framework.Is.EqualTo(1));
        }

        [TestCase(10, 1)]
        [TestCase(60, 2)]
        [TestCase(100, 2)]
        public void PagingTests(int messages, int pages)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < messages; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, sb.ToString());
            this.controller.StartReadLog();
            this.WaitReadingComplete();
            Assert.That(this.controller.TotalPages, NUnit.Framework.Is.EqualTo(pages));
        }

        [TestCase("test", false, true)]
        [TestCase("t[", false, true)]
        [TestCase("t[", true, false)]
        [TestCase("t[1]", true, true)]
        [TestCase(null, true, true)]
        [TestCase(null, false, true)]
        public void FilterValidation(string filter, bool useRegex, bool result)
        {
            Assert.That(MainController.IsValidFilter(filter, useRegex), NUnit.Framework.Is.EqualTo(result));
        }
    }
}