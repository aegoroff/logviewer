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
            this.controller = new MainController(this.view, MessageStart, RecentPath);
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

        private const string TestPath = "f";
        private const string RecentPath = "r";
        private Mockery mockery;
        private ILogView view;
        private MainController controller;
        private const string LogPathProperty = "LogPath";
        private const string ClearRecentFilesListMethod = "ClearRecentFilesList";
        private const string CreateRecentFileItemMethod = "CreateRecentFileItem";
        private const string MessageExamples = "2008-12-27 19:31:47,250 [4688] INFO \n2008-12-27 19:40:11,906 [5272] ERROR ";

        private const string MessageStart = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}.*";
        private const string OpenLogFileMethod = "OpenLogFile";
        private const string LogFileNameProperty = "LogFileName";
        private const string IsBusyProperty = "IsBusy";
        private const string ReadLogMethod = "ReadLog";
        private const string LoadLogMethod = "LoadLog";

        private static Stream CreateTestStream(string data)
        {
            var result = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(data);
            result.Write(bytes, 0, bytes.Length);
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }

        [Test]
        public void FilterText()
        {
            this.controller.TextFilter(".*5272.*");
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void LoadLog()
        {
            File.WriteAllText(TestPath, string.Empty);
            Expect.Once.On(this.view).Method(LoadLogMethod).With(TestPath);
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
        public void MaxAndMaxFilter()
        {
            this.controller.MinFilter("WARN");
            this.controller.MaxFilter("WARN");
            this.controller.ReadLog(CreateTestStream(MessageExamples + "\n2008-12-27 19:42:11,906 [5272] WARN "));
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(1));
        }

        [TestCase("WARN", 1)]
        [TestCase(@"\[?WARN\]?", 1)]
        [TestCase(@"\[?WERN\]?", 2)]
        [TestCase(@"ERROR", 2)]
        public void MaxFilter(string f, int c)
        {
            this.controller.MaxFilter(f);
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(c));
        }

        [TestCase(@"\[?WARN\]?", "WARN", 1)]
        [TestCase(@"\[?WARN\]?", "WaRN", 2)]
        public void MaxFilterWithCustomMarker(string m, string f, int c)
        {
            this.controller.DefineWarnMarker(m);
            this.controller.MaxFilter(f);
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(c));
        }

        [TestCase("WARN", 1)]
        [TestCase(@"\[?WARN\]?", 1)]
        [TestCase(@"\[?WERN\]?", 2)]
        [TestCase("INFO", 2)]
        public void MinFilter(string f, int c)
        {
            this.controller.MinFilter(f);
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(c));
        }

        [TestCase(@"\[?WARN\]?", "WARN", 1)]
        [TestCase(@"\[?WARN\]?", "WaRN", 2)]
        public void MinFilterWithCustomMarker(string m, string f, int c)
        {
            this.controller.DefineWarnMarker(m);
            this.controller.MinFilter(f);
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(c));
        }

        [Test]
        public void MinFilterGreaterThenMax()
        {
            this.controller.MinFilter("ERROR");
            this.controller.MaxFilter("WARN");
            this.controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(0));
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
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(0));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("0 Bytes"));
        }

        [Test]
        public void ReadFromBadPath()
        {
            Expect.Never.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            Expect.Never.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(string.Empty));
            Assert.IsEmpty(this.controller.ReadLog());
            Assert.IsEmpty(this.controller.Messages);
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
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(2000));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("74,22 Kb (76000 Bytes)"));
        }

        [Test]
        public void ReadNormalLog()
        {
            File.WriteAllText(TestPath, MessageExamples);
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(2));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("74 Bytes"));
        }

        [Test]
        public void ReadNormalLogWin1251()
        {
            const string messageExamples1251 = "2008-12-27 19:31:47,250 [4688] INFO \n2008-12-27 19:40:11,906 [тест] ERROR ";
            File.WriteAllText(TestPath, messageExamples1251, Encoding.GetEncoding("windows-1251"));
            Assert.IsNotEmpty(this.controller.ReadLog());
            Assert.That(MessageExamples.Length, NUnit.Framework.Is.EqualTo(messageExamples1251.Length));
            Assert.That(this.controller.Messages.Count, NUnit.Framework.Is.EqualTo(2));
            Assert.That(this.controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("84 Bytes"));
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
            Expect.Exactly(2).On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            Expect.Once.On(this.view).Method(ClearRecentFilesListMethod);
            Expect.Once.On(this.view).Method(CreateRecentFileItemMethod).Will(Return.Value(TestPath));
            this.controller.SaveRecentFiles();
            this.controller.ReadRecentFiles();
        }

        [Test]
        public void SaveRecentFiles()
        {
            Expect.Exactly(2).On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            this.controller.SaveRecentFiles();
        }
    }
}