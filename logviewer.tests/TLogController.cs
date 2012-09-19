using System.IO;
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

        private const string MessageStart = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}.*";

        [Test]
        public void ReadEmptyFile()
        {
            File.Create(TestPath).Dispose();
            Expect.AtLeastOnce.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(TestPath));
            var controller = new LogController(this.view, MessageStart);
            Assert.IsNotEmpty(controller.ReadLog(TestPath));
            Assert.That(controller.LogSize, NUnit.Framework.Is.EqualTo(3));
        }

        [Test]
        public void ReadFromBadPath()
        {
            Expect.Once.On(this.view).GetProperty(LogPathProperty).Will(Return.Value(string.Empty));
            var controller = new LogController(this.view, MessageStart);
            Assert.IsEmpty(controller.ReadLog(string.Empty));
        }
    }
}