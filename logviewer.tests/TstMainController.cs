using System;
using System.IO;
using System.Text;
using NMock2;
using NUnit.Framework;
using logviewer.core;

namespace logviewer.tests
{
    [TestFixture]
    public class TstMainController
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            this.mockery = new Mockery();
            this.view = this.mockery.NewMock<ILogView>();
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            this.controller = new MainController(this.view, MessageStart, RecentPath, this.levels);
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
        }

        #endregion

        private readonly string[] levels = new[]
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
        private Mockery mockery;
        private ILogView view;
        private MainController controller;
        private const string LogPathProperty = "LogPath";
        private const string ClearRecentFilesListMethod = "ClearRecentFilesList";
        private const string CreateRecentFileItemMethod = "CreateRecentFileItem";
        private const string MessageExamples = "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1\n2008-12-27 19:40:11,906 [5272] ERROR \nmessage body 2";

        private const string MessageStart = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}.*";
        private const string OpenLogFileMethod = "OpenLogFile";
        private const string LogFileNameProperty = "LogFileName";
        private const string IsBusyProperty = "IsBusy";
        private const string ReadLogMethod = "ReadLog";
        private const string LoadLogMethod = "LoadLog";
        private const string SetCurrentPageMethod = "SetCurrentPage";
        private const string DisableBackMethod = "DisableBack";
        private const string DisableForwardMethod = "DisableForward";

        private static Stream CreateTestStream(string data)
        {
            var result = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(data);
            result.Write(bytes, 0, bytes.Length);
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }

        [TestCase((int) LogLevel.Warn, (int) LogLevel.Warn, MessageExamples + "\n2008-12-27 19:42:11,906 [5272] WARN ", 1)]
        [TestCase((int) LogLevel.Error, (int) LogLevel.Warn, MessageExamples, 0)]
        public void MaxAndMaxFilter(int min, int max, string msg, int count)
        {
            this.controller.MinFilter(min);
            this.controller.MaxFilter(max);
            this.controller.ReadLog(CreateTestStream(msg));
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(count));
        }

        [TestCase(LogLevel.Warn, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 1)]
        [TestCase(LogLevel.Info, new[] { "TRACE", "DEBUG", @"\[?INFO\]?", "WARN", "ERROR", "FATAL" }, 1)]
        [TestCase(LogLevel.Debug, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 0)]
        [TestCase(LogLevel.Error, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 2)]
        [TestCase(LogLevel.Trace, new[] { "TRACE", "DEBUG", @"\[?InFO\]?", "WARN", "ERROR", "FATAL" }, 1)]
        public void MaxFilter(LogLevel filter, string[] markers, int c)
        {
            this.controller = new MainController(this.view, MessageStart, RecentPath, markers);
            this.controller.MaxFilter((int)filter);
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(c));
        }
        
        [TestCase(LogLevel.Warn, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 1)]
        [TestCase(LogLevel.Error, new[] { "TRACE", "DEBUG", "INFO", "WARN", @"\[?ERROR\]?", "FATAL" }, 1)]
        [TestCase(LogLevel.Info, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 2)]
        [TestCase(LogLevel.Fatal, new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL" }, 0)]
        [TestCase(LogLevel.Error, new[] { "TRACE", "DEBUG", "INFO", "WARN", @"\[?ERRoR\]?", "FATAL" }, 0)]
        public void MinFilter(LogLevel filter, string[] markers, int c)
        {
            this.controller = new MainController(this.view, MessageStart, RecentPath, markers);
            this.controller.MinFilter((int)filter);
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(c));
        }

        [Test]
        public void ExportRtfFail()
        {
            Expect.Once.On(this.view).Method("OpenExport").With(TestPath + ".rtf").Will(Return.Value(false));
            this.controller.ExportToRtf();
        }

        [Test]
        public void ExportRtfSuccess()
        {
            Expect.Once.On(this.view).Method("OpenExport").With(TestPath + ".rtf").Will(Return.Value(true));
            Expect.Once.On(this.view).Method("SaveRtf");
            this.controller.ExportToRtf();
        }

        [Test]
        public void FilterText()
        {
            this.controller.TextFilter(".*5272.*");
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void LoadLog()
        {
            File.WriteAllText(TestPath, string.Empty);
            Expect.Once.On(this.view).Method(LoadLogMethod).With(TestPath);
            Expect.Once.On(this.view).Method(SetCurrentPageMethod).With(1);
            Expect.Once.On(this.view).Method(DisableBackMethod).With(true);
            Expect.Once.On(this.view).Method(DisableForwardMethod).With(true);
            Assert.That(File.Exists(TestPath), NUnit.Framework.Is.True);
            this.controller.LoadLog(TestPath);
            Assert.That(File.Exists(TestPath), NUnit.Framework.Is.False);
        }

        [Test]
        public void LoadLogEmpty()
        {
            Expect.Never.On(this.view).Method(LoadLogMethod).WithAnyArguments();
            this.controller.LoadLog(string.Empty);
        }

        [Test]
        public void LoadLogThatThrows()
        {
            File.WriteAllText(TestPath, string.Empty);
            Expect.Once.On(this.view).Method(LoadLogMethod).Will(Throw.Exception(new Exception()));
            Expect.Once.On(this.view).Method(DisableBackMethod).With(true);
            Expect.Once.On(this.view).Method(DisableForwardMethod).With(true);
            Assert.That(File.Exists(TestPath), NUnit.Framework.Is.True);
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
            Assert.That(File.Exists(TestPath), NUnit.Framework.Is.False);
        }

        [Test]
        public void LoadLogUnexistPath()
        {
            Expect.Never.On(this.view).Method(LoadLogMethod).WithAnyArguments();
            this.controller.LoadLog(TestPath);
        }

        [Test]
        public void OpenLogFileCanceled()
        {
            Expect.Once.On(this.view).Method(OpenLogFileMethod).Will(Return.Value(false));
            this.controller.OpenLogFile();
        }

        [Test]
        public void OpenLogFileOpenedAndBusy()
        {
            Expect.Once.On(this.view).Method(OpenLogFileMethod).Will(Return.Value(true));
            Expect.Once.On(this.view).GetProperty(LogFileNameProperty).Will(Return.Value(TestPath));
            Expect.Once.On(this.view).SetProperty(LogPathProperty).To(TestPath);
            Expect.Once.On(this.view).GetProperty(IsBusyProperty).Will(Return.Value(true));
            this.controller.OpenLogFile();
        }

        [Test]
        public void OpenLogFileOpenedAndNotBusy()
        {
            Expect.Once.On(this.view).Method(OpenLogFileMethod).Will(Return.Value(true));
            Expect.Once.On(this.view).GetProperty(LogFileNameProperty).Will(Return.Value(TestPath));
            Expect.Once.On(this.view).SetProperty(LogPathProperty).To(TestPath);
            Expect.Once.On(this.view).GetProperty(IsBusyProperty).Will(Return.Value(false));
            Expect.Once.On(this.view).Method(ReadLogMethod);
            this.controller.OpenLogFile();
        }

        [Test]
        public void ReadEmptyFile()
        {
            File.Create(TestPath).Dispose();
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.LogSize, NUnit.Framework.Is.EqualTo(0));
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(0));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("0 Bytes"));
        }
        
        [Test]
        [ExpectedException(typeof(ArgumentException))]
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
            Expect.Never.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            Expect.Never.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(string.Empty));
            Assert.IsEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(0));
            Assert.IsNull(this.controller.HumanReadableLogSize);
        }

        [Test]
        public void ReadNormalBigLog()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 1000; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            Assert.IsNotEmpty(this.controller.ReadLog(CreateTestStream(sb.ToString())));
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2000));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("103,52 Kb (106000 Bytes)"));
        }

        [Test]
        public void ReadNormalLog()
        {
            File.WriteAllText(TestPath, MessageExamples);
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("104 Bytes"));
        }

        [Test]
        public void ReadNormalLogWin1251()
        {
            File.WriteAllText(TestPath, MessageExamples, Encoding.GetEncoding("windows-1251"));
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("104 Bytes"));
        }

        [Test]
        public void ReadNotEmptyLogWithoutMessagesMatch()
        {
            File.WriteAllText(TestPath, "test log");
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("8 Bytes"));
        }

        [Test]
        public void ReadRecentFiles()
        {
            Expect.Once.On(this.view).Method(ClearRecentFilesListMethod);
            this.controller.ReadRecentFiles();
        }

        [Test]
        public void SaveAndReadRecentFiles()
        {
            Expect.Once.On(this.view).Method(ClearRecentFilesListMethod);
            Expect.Once.On(this.view).Method(CreateRecentFileItemMethod).Will(Return.Value(TestPath));
            this.controller.SaveRecentFiles();
            this.controller.ReadRecentFiles();
        }

        [Test]
        public void SaveRecentFiles()
        {
            this.controller.SaveRecentFiles();
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
            for (var i = 0; i < 1000; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            Assert.IsNotEmpty(this.controller.ReadLog(CreateTestStream(sb.ToString())));
            Assert.That(this.controller.TotalPages, NUnit.Framework.Is.EqualTo(1));
        }
        
        [Test]
        public void TotalPagesMessagesInexactlyAbovePageSize()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 6000; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            Assert.IsNotEmpty(this.controller.ReadLog(CreateTestStream(sb.ToString())));
            Assert.That(this.controller.TotalPages, NUnit.Framework.Is.EqualTo(2));
        }
        
        [Test]
        public void TotalPagesMessagesExactlyAbovePageSize()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 10000; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            Assert.IsNotEmpty(this.controller.ReadLog(CreateTestStream(sb.ToString())));
            Assert.That(this.controller.TotalPages, NUnit.Framework.Is.EqualTo(2));
        }
    }
}