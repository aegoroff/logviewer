using System;
using System.IO;
using System.Text;
using NMock;
using NUnit.Framework;
using logviewer.core;

namespace logviewer.tests
{
    [TestFixture]
    public class TstMainController
    {
        private const string f1 = "1";
        private const string f2 = "2";

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            this.mockery = new MockFactory();
            this.view = this.mockery.CreateMock<ILogView>();
            this.controller = new MainController(MessageStart, RecentPath, this.levels);
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
            if (File.Exists(f1))
            {
                File.Delete(f1);
            }
            if (File.Exists(f2))
            {
                File.Delete(f2);
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
        private MockFactory mockery;
        private Mock<ILogView> view;
        private MainController controller;
        private const string MessageExamples = "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1\n2008-12-27 19:40:11,906 [5272] ERROR \nmessage body 2";

        private const string MessageStart = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}.*";

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
            this.view.Expects.One.Method(_ => _.Initialize());
            this.controller = new MainController(MessageStart, RecentPath, markers);
            this.controller.SetView(this.view.MockObject);
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
            this.view.Expects.One.Method(_ => _.Initialize());
            this.controller = new MainController(MessageStart, RecentPath, markers);
            this.controller.SetView(this.view.MockObject);
            this.controller.MinFilter((int)filter);
            this.controller.ReadLog(CreateTestStream(MessageExamples));
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
            this.controller.TextFilter(".*5272.*");
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }
        
        [Test]
        public void FilterTextNotContainsTextInHead()
        {
            this.controller.TextFilter("^(?!.*ERROR).*");
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }
        
        [Test]
        public void FilterTextNotContainsTextInBody()
        {
            this.controller.TextFilter("^(?!.*message body 2).*");
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void LoadLog()
        {
            File.WriteAllText(TestPath, string.Empty);
            this.view.Expects.One.MethodWith(v => v.LoadLog(TestPath));
            this.view.Expects.One.MethodWith(v => v.SetCurrentPage(1));
            this.view.Expects.One.MethodWith(v => v.DisableBack(true));
            this.view.Expects.One.MethodWith(v => v.DisableForward(true));

            Assert.That(File.Exists(TestPath), NUnit.Framework.Is.True);
            this.controller.LoadLog(TestPath);
            Assert.That(File.Exists(TestPath), NUnit.Framework.Is.False);
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
            File.WriteAllText(TestPath, string.Empty);
            this.view.Expects.One.MethodWith(v => v.LoadLog(TestPath)).Will(Throw.Exception(new Exception()));
            this.view.Expects.One.MethodWith(v => v.DisableBack(true));
            this.view.Expects.One.MethodWith(v => v.DisableForward(true));

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
            this.view.Expects.No.Method(v => v.LoadLog(null)).WithAnyArguments();
            this.controller.LoadLog(TestPath);
        }

        [Test]
        public void OpenLogFileCanceled()
        {
            this.view.Expects.One.Method(v => v.OpenLogFile()).WillReturn(false);
            this.controller.OpenLogFile();
        }

        [Test]
        public void OpenLogFileOpenedAndBusy()
        {
            this.view.Expects.One.Method(v => v.OpenLogFile()).WillReturn(true);
            this.view.Expects.One.GetProperty(v => v.LogFileName).WillReturn(TestPath);
            this.view.Expects.One.SetProperty(v => v.LogPath).To(TestPath);
            this.view.Expects.One.GetProperty(v => v.IsBusy).WillReturn(true);

            this.controller.OpenLogFile();
        }

        [Test]
        public void OpenLogFileOpenedAndNotBusy()
        {
            this.view.Expects.One.Method(v => v.OpenLogFile()).WillReturn(true);
            this.view.Expects.One.GetProperty(v => v.LogFileName).WillReturn(TestPath);
            this.view.Expects.One.SetProperty(v => v.LogPath).To(TestPath);
            this.view.Expects.One.GetProperty(v => v.IsBusy).WillReturn(false);
            this.view.Expects.One.Method(v => v.ReadLog());
            this.controller.OpenLogFile();
        }

        [Test]
        public void ReadEmptyFile()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
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
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.No.GetProperty(v => v.LogPath).WillReturn(TestPath);
            this.view.Expects.No.GetProperty(v => v.LogPath).WillReturn(string.Empty);
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
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples);
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("104 Bytes"));
        }

        [Test]
        public void ReadNormalLogWin1251()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, MessageExamples, Encoding.GetEncoding("windows-1251"));
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(2));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("104 Bytes"));
        }

        [Test]
        public void ReadNotEmptyLogWithoutMessagesMatch()
        {
            this.view.Expects.AtLeastOne.GetProperty(v => v.LogPath).WillReturn(TestPath);
            File.WriteAllText(TestPath, "test log");
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.MessagesCount, NUnit.Framework.Is.EqualTo(1));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("8 Bytes"));
        }

        [Test]
        public void ReadRecentFilesNoFile()
        {
            this.view.Expects.One.Method(v => v.ClearRecentFilesList());
            this.controller.ReadRecentFiles();
        }
        
        [Test]
        public void ReadRecentFilesSingleUnexistFile()
        {
            File.WriteAllLines(RecentPath, new[] { "1" });
            this.view.Expects.One.Method(v => v.ClearRecentFilesList());
            this.controller.ReadRecentFiles();
        }
        
        [Test]
        public void ReadRecentFilesSingleExistFile()
        {
            
            using (File.Open(f1, FileMode.Create))
            {
            }
            File.WriteAllLines(RecentPath, new[] { f1 });
            this.view.Expects.One.Method(v => v.ClearRecentFilesList());
            this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f1));
            this.controller.ReadRecentFiles();
        }
        
        [Test]
        public void ReadRecentFilesManyExistFile()
        {
            
            using (File.Open(f1, FileMode.Create))
            {
            }
            using (File.Open(f2, FileMode.Create))
            {
            }
            File.WriteAllLines(RecentPath, new[] { f1, f2 });
            using (mockery.Ordered())
            {
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f2));
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f1));
            }
            this.controller.ReadRecentFiles();
        }

        [Test]
        public void SaveAndReadRecentFilesNoFile()
        {
            using (mockery.Ordered())
            {
                this.view.Expects.Exactly(2).GetProperty(v => v.LogPath).WillReturn(TestPath);
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.No.MethodWith(v => v.CreateRecentFileItem(TestPath));
            }
            this.controller.SaveRecentFiles();
            this.controller.ReadRecentFiles();
        }
        
        [Test]
        public void SaveAndReadRecentFiles()
        {
            using (File.Open(TestPath, FileMode.Create))
            {
            }
            using (mockery.Ordered())
            {
                this.view.Expects.Exactly(2).GetProperty(v => v.LogPath).WillReturn(TestPath);
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(TestPath));
            }
            this.controller.SaveRecentFiles();
            this.controller.ReadRecentFiles();
        }
        
        [Test]
        public void SaveAndReadRecentFilesSeveralTimes()
        {
            using (File.Open(TestPath, FileMode.Create))
            {
            }
            using (File.Open(f1, FileMode.Create))
            {
            }
            using (File.Open(f2, FileMode.Create))
            {
            }

            using (mockery.Ordered())
            {
                this.view.Expects.One.Method(_ => _.Initialize());
                this.view.Expects.Exactly(2).GetProperty(v => v.LogPath).WillReturn(TestPath);
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(TestPath));
                this.view.Expects.Exactly(2).GetProperty(v => v.LogPath).WillReturn(f1);
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f1));
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(TestPath));
                this.view.Expects.Exactly(2).GetProperty(v => v.LogPath).WillReturn(f2);
                this.view.Expects.One.Method(v => v.ClearRecentFilesList());
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f2));
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(f1));
                this.view.Expects.One.MethodWith(v => v.CreateRecentFileItem(TestPath));
            }
            this.controller = new MainController(MessageStart, RecentPath, this.levels);
            this.controller.SetView(this.view.MockObject);
            this.controller.SaveRecentFiles();
            this.controller.ReadRecentFiles();
            this.controller.SaveRecentFiles();
            this.controller.ReadRecentFiles();
            this.controller.SaveRecentFiles();
            this.controller.ReadRecentFiles();
        }

        [Test]
        public void SaveRecentFiles()
        {
            this.view.Expects.Exactly(2).GetProperty(v => v.LogPath).WillReturn(TestPath);
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