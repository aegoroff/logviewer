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
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(TestPath))
            {
                File.Delete(TestPath);
            }
        }

        #endregion

        private const string TestPath = "f";
        private Mockery mockery;
        private ILogView view;
        private const string LogPathProperty = "LogPath";
        const string MessageExamples = "2008-12-27 19:31:47,250 [4688] INFO \n2008-12-27 19:40:11,906 [5272] ERROR ";

        private const string MessageStart = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}.*";

        [Test]
        public void ReadEmptyFile()
        {
            File.Create(TestPath).Dispose();
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            var controller = new LogController(this.view, MessageStart, null);
            Assert.IsNotEmpty(controller.ReadLog(TestPath));
            Assert.That(controller.LogSize, NUnit.Framework.Is.EqualTo(3));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(0));
            Assert.That(controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("3 Bytes"));
        }

        [Test]
        public void ReadFromBadPath()
        {
            Expect.Once.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(string.Empty));
            var controller = new LogController(this.view, MessageStart, null);
            Assert.IsEmpty(controller.ReadLog(string.Empty));
            Assert.IsEmpty(controller.Messages);
            Assert.IsNull(controller.HumanReadableLogSize);
        }

        [Test]
        public void ReadNormalLog()
        {
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            File.WriteAllText(TestPath, MessageExamples);
            var controller = new LogController(this.view, MessageStart, null);
            Assert.IsNotEmpty(controller.ReadLog(TestPath));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(2));
            Assert.That(controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("77 Bytes"));
        }
        
        [Test]
        public void ReadNormalBigLog()
        {
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            var sb = new StringBuilder();
            for (var i = 0; i < 10000; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            File.WriteAllText(TestPath, sb.ToString());
            var controller = new LogController(this.view, MessageStart, null);
            Assert.IsNotEmpty(controller.ReadLog(TestPath));
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(20000));
            Assert.That(controller.HumanReadableLogSize, NUnit.Framework.Is.EqualTo("742,19 Kb (760003 Bytes)"));
        }

        [Test]
        public void MinFilter()
        {
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            File.WriteAllText(TestPath, MessageExamples);
            var controller = new LogController(this.view, MessageStart, null);
            controller.MinFilter("WARN");
            controller.ReadLog(TestPath);
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(1));
        }

        [Test]
        public void MinFilterBorder()
        {
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            File.WriteAllText(TestPath, MessageExamples);
            var controller = new LogController(this.view, MessageStart, null);
            controller.MinFilter("INFO");
            controller.ReadLog(TestPath);
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(2));
        }
        
        [Test]
        public void MaxFilter()
        {
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            File.WriteAllText(TestPath, MessageExamples);
            var controller = new LogController(this.view, MessageStart, null);
            controller.MaxFilter("WARN");
            controller.ReadLog(TestPath);
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(1));
        }
        
        [Test]
        public void MaxFilterBorder()
        {
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            File.WriteAllText(TestPath, MessageExamples);
            var controller = new LogController(this.view, MessageStart, null);
            controller.MaxFilter("ERROR");
            controller.ReadLog(TestPath);
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(2));
        }

        [Test]
        public void MinFilterGreaterThenMax()
        {
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            File.WriteAllText(TestPath, MessageExamples);
            var controller = new LogController(this.view, MessageStart, null);
            controller.MinFilter("ERROR");
            controller.MaxFilter("WARN");
            controller.ReadLog(TestPath);
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(0));
        }
        
        [Test]
        public void MaxAndMaxFilter()
        {
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            File.WriteAllText(TestPath, MessageExamples + "\n2008-12-27 19:42:11,906 [5272] WARN ");
            var controller = new LogController(this.view, MessageStart, null);
            controller.MinFilter("WARN");
            controller.MaxFilter("WARN");
            controller.ReadLog(TestPath);
            Assert.That(controller.Messages.Count, NUnit.Framework.Is.EqualTo(1));
        }
    }
}