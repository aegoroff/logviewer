using System.IO;
using System.Text;
using NMock2;
using NUnit.Framework;
using logviewer.core;

namespace logviewer.tests
{
    [TestFixture]
    public class TLogController
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            this.mockery = new Mockery();
            this.view = this.mockery.NewMock<ILogView>();
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            this.controller = new LogController(this.view, MessageStart, RecentPath);
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
        private LogController controller;
        private const string LogPathProperty = "LogPath";
        private const string ClearRecentFilesListMethod = "ClearRecentFilesList";
        private const string CreateRecentFileItemMethod = "CreateRecentFileItem";
        const string MessageExamples = "2008-12-27 19:31:47,250 [4688] INFO \n2008-12-27 19:40:11,906 [5272] ERROR ";

        private const string MessageStart = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}.*";

        [Test]
        public void ReadEmptyFile()
        {
            File.Create(TestPath).Dispose();
            Assert.IsNotEmpty(controller.ReadLog());
            Assert.That(controller.LogSize, NUnit.Framework.Is.EqualTo(0));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(0));
            Assert.That(controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("0 Bytes"));
        }

        [Test]
        public void ReadFromBadPath()
        {
            Expect.Never.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            Expect.Never.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(string.Empty));
            Assert.IsEmpty(controller.ReadLog());
            Assert.IsEmpty(controller.Messages);
            Assert.IsNull(controller.HumanReadableLogSize);
        }

        [Test]
        public void ReadNormalLog()
        {
            File.WriteAllText(TestPath, MessageExamples);
            Assert.IsNotEmpty(controller.ReadLog());
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(2));
            Assert.That(controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("80 Bytes"));
        }
        
        [Test]
        public void ReadNormalBigLog()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 1000; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            Assert.IsNotEmpty(controller.ReadLog(CreateTestStream(sb.ToString())));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(2000));
            Assert.That(controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("74,22 Kb (76000 Bytes)"));
        }

        [Test]
        public void MinFilter()
        {
            controller.MinFilter("WARN");
            controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void MinFilterBorder()
        {
            controller.MinFilter("INFO");
            controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(2));
        }
        
        [Test]
        public void MaxFilter()
        {
            controller.MaxFilter("WARN");
            controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(1));
        }
        
        [Test]
        public void MaxFilterBorder()
        {
            controller.MaxFilter("ERROR");
            controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(2));
        }

        [Test]
        public void MinFilterGreaterThenMax()
        {
            controller.MinFilter("ERROR");
            controller.MaxFilter("WARN");
            controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(0));
        }
        
        [Test]
        public void MaxAndMaxFilter()
        {
            controller.MinFilter("WARN");
            controller.MaxFilter("WARN");
            controller.ReadLog(CreateTestStream(MessageExamples + "\n2008-12-27 19:42:11,906 [5272] WARN "));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void FilterText()
        {
            controller.TextFilter(".*5272.*");
            controller.ReadLog(CreateTestStream(MessageExamples));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(1));
        }

        static Stream CreateTestStream(string data)
        {
            var result = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(data);
            result.Write(bytes, 0, bytes.Length);
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }

        [Test]
        public void ReadRecentFiles()
        {
            Expect.Once.On(this.view).Method(ClearRecentFilesListMethod);
            controller.ReadRecentFiles();
        }
        
        [Test]
        public void SaveRecentFiles()
        {
            Expect.Exactly(2).On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            controller.SaveRecentFiles();
        }
        
        [Test]
        public void SaveAndReadRecentFiles()
        {
            Expect.Exactly(2).On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            Expect.Once.On(this.view).Method(ClearRecentFilesListMethod);
            Expect.Once.On(this.view).Method(CreateRecentFileItemMethod).Will(Return.Value(TestPath));
            controller.SaveRecentFiles();
            controller.ReadRecentFiles();
        }
    }
}