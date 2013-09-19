// Created by: egr
// Created at: 19.09.2012
// © 2012-2013 Alexander Egorov

using System;
using System.IO;
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
            CleanupTestFiles();
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            this.mockery = new MockFactory();
            this.view = this.mockery.CreateMock<ILogView>();
            this.controller = new MainController(MessageStart, this.levels, SettingsDb, KeepLastNFiles, 100);
            this.view.Expects.One.Method(_ => _.Initialize());
            this.controller.SetView(this.view.MockObject);
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(TestPath))
            {
                File.Delete(TestPath);
            }
            if (File.Exists(RecentPath))
            {
                File.Delete(RecentPath);
            }
            CleanupTestFiles();
            var settingsDatabaseFilePath = Path.Combine(Settings.ApplicationFolder, SettingsDb);
            if (File.Exists(settingsDatabaseFilePath))
            {
                File.Delete(settingsDatabaseFilePath);
            }
            this.controller.Dispose();
        }

        private static void CleanupTestFiles()
        {
            if (File.Exists(f1))
            {
                File.Delete(f1);
            }
            if (File.Exists(f2))
            {
                File.Delete(f2);
            }
            if (File.Exists(f3))
            {
                File.Delete(f3);
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

            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }

        [TestCase((int)LogLevel.Warn, (int)LogLevel.Warn, MessageExamples + "\n2008-12-27 19:42:11,906 [5272] WARN ", 1)
        ]
        [TestCase((int)LogLevel.Warn, (int)LogLevel.Warn, MessageExamples, 0)]
        public void MaxAndMaxFilter(int min, int max, string msg, int count)
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, msg);
            this.controller.MinFilter(min);
            this.controller.MaxFilter(max);
            Assert.IsNotEmpty(this.controller.ReadLog());
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
            this.controller = new MainController(MessageStart, markers, SettingsDb, KeepLastNFiles);
            this.controller.SetView(this.view.MockObject);
            this.controller.MaxFilter((int)filter);
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(c));
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
            this.controller = new MainController(MessageStart, markers, SettingsDb, KeepLastNFiles);
            this.controller.SetView(this.view.MockObject);
            this.controller.MinFilter((int)filter);
            Assert.IsNotEmpty(this.controller.ReadLog());
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

        [Test]
        public void FilterText()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples);
            this.controller.TextFilter(".*5272.*");
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void FilterTextNotContainsTextInHead()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples);
            this.controller.TextFilter(".*ERROR.*");
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void FilterTextNotContainsTextInBody()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples);
            this.controller.TextFilter(".*message body 2.*");
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void FilterTextNotContainsTextInBodySubstr()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples);
            this.controller.TextFilter("message body 2");
            this.controller.UserRegexp(false);
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void LoadLog()
        {
            this.view.Expects.One.MethodWith(v => v.LoadLog(TestPath));
            this.view.Expects.One.MethodWith(v => v.SetCurrentPage(1));
            this.view.Expects.One.MethodWith(v => v.DisableBack(true));
            this.view.Expects.One.MethodWith(v => v.DisableForward(true));

            this.controller.LoadLog(TestPath);
        }

        [Test]
        public void LoadLogEmpty()
        {
            this.view.Expects.No.Method(v => v.LoadLog(null)).WithAnyArguments();
            this.controller.LoadLog(string.Empty);
        }

        [Test]
        public void LoadLogThatThrows()
        {
            this.view.Expects.One.MethodWith(v => v.LoadLog(TestPath)).Will(Throw.Exception(new Exception()));
            this.view.Expects.One.MethodWith(v => v.DisableBack(true));
            this.view.Expects.One.MethodWith(v => v.DisableForward(true));

            var thrown = false;
            try
            {
                this.controller.LoadLog(TestPath);
            }
            catch (Exception)
            {
                thrown = true;
            }
            Assert.That(thrown, NUnit.Framework.Is.True);
        }

        [Test]
        public void OpenLogFileCanceled()
        {
            this.view.Expects.One.Method(v => v.OpenLogFile()).WillReturn(false);
            this.controller.OpenLogFile();
        }

        [Test]
        public void ReadEmptyFile()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.Create(TestPath).Dispose();
            Assert.IsEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(0));
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void ReadEmptyWhenMinGreaterThenMax()
        {
            this.controller.MinFilter((int)LogLevel.Error);
            this.controller.MaxFilter((int)LogLevel.Info);
            File.Create(TestPath).Dispose();
            this.controller.ReadLog();
        }

        [Test]
        public void ReadFromBadPath()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.No.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.No.GetProperty(v => v.LogPath).WillReturn(string.Empty);
            Assert.IsEmpty(this.controller.ReadLog());
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
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2000));
        }

        [Test]
        public void ReadNormalLog()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples);
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2));
        }

        [Test]
        public void ReadNormalLogWin1251()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples, Encoding.GetEncoding("windows-1251"));
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2));
        }

        [Test]
        public void ReadNotEmptyLogWithoutMessagesMatch()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, "test log");
            Assert.IsNotEmpty(this.controller.ReadLog());
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
            using (File.Open(f1, FileMode.Create))
            {
            }
            using (File.Open(f2, FileMode.Create))
            {
            }
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
            using (File.Open(f1, FileMode.Create))
            {
            }
            using (File.Open(f2, FileMode.Create))
            {
            }
            using (File.Open("3", FileMode.Create))
            {
            }
            using (this.mockery.Ordered())
            {
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(f1);
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn(f2);
                this.view.Expects.One.GetProperty(v => v.LogPath).WillReturn("3");
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem("3"));
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
            using (File.Open(f1, FileMode.Create))
            {
            }
            using (File.Open(f2, FileMode.Create))
            {
            }
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

        [Test]
        public void TotalPagesMessagesBelowPageSize()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 10; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, sb.ToString());
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.TotalPages, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void TotalPagesMessagesInexactlyAbovePageSize()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 60; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, sb.ToString());
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.TotalPages, NUnit.Framework.Is.EqualTo(2));
        }

        [Test]
        public void TotalPagesMessagesExactlyAbovePageSize()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 100; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, sb.ToString());
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.TotalPages, NUnit.Framework.Is.EqualTo(2));
        }
    }
}