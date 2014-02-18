// Created by: egr
// Created at: 11.01.2014
// © 2012-2014 Alexander Egorov

using System.IO;
using logviewer.core;
using NUnit.Framework;

namespace logviewer.tests
{
    [TestFixture]
    public class TstSqliteSettingsProvider
    {
        [SetUp]
        public void Setup()
        {
            this.provider = new SqliteSettingsProvider(dbPath, TstMainController.Levels, TstMainController.MessageStart,
                100, 5);
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }

        private static readonly string dbPath = Path.GetTempFileName();
        private SqliteSettingsProvider provider;

        [Test]
        public void KeepLastNFiles()
        {
            Assert.That(this.provider.KeepLastNFiles, Is.EqualTo(5));
        }

        [Test]
        public void PageSize()
        {
            Assert.That(this.provider.PageSize, Is.EqualTo(100));
        }

        [Test]
        public void ReadParsingTemplate()
        {
            ParsingTemplate template = this.provider.ReadParsingTemplate();
            Assert.That(template.Index, Is.EqualTo(0));
            Assert.That(template.Trace, Is.EqualTo(TstMainController.Levels[0]));
            Assert.That(template.Debug, Is.EqualTo(TstMainController.Levels[1]));
            Assert.That(template.Info, Is.EqualTo(TstMainController.Levels[2]));
            Assert.That(template.Warn, Is.EqualTo(TstMainController.Levels[3]));
            Assert.That(template.Error, Is.EqualTo(TstMainController.Levels[4]));
            Assert.That(template.Fatal, Is.EqualTo(TstMainController.Levels[5]));
            Assert.That(template.StartMessage, Is.EqualTo(TstMainController.MessageStart));
        }

        [Test]
        public void UpdateParsingTemplate()
        {
            ParsingTemplate template = this.provider.ReadParsingTemplate();
            template.Trace += "1";
            template.Debug += "1";
            template.Info += "1";
            template.Warn += "1";
            template.Error += "1";
            template.Fatal += "1";
            template.StartMessage += "1";

            this.provider.UpdateParsingProfile(template);

            ParsingTemplate template1 = this.provider.ReadParsingTemplate();

            Assert.That(template1.Trace, Is.EqualTo(TstMainController.Levels[0] + "1"));
            Assert.That(template1.Debug, Is.EqualTo(TstMainController.Levels[1] + "1"));
            Assert.That(template1.Info, Is.EqualTo(TstMainController.Levels[2] + "1"));
            Assert.That(template1.Warn, Is.EqualTo(TstMainController.Levels[3] + "1"));
            Assert.That(template1.Error, Is.EqualTo(TstMainController.Levels[4] + "1"));
            Assert.That(template1.Fatal, Is.EqualTo(TstMainController.Levels[5] + "1"));
            Assert.That(template.StartMessage, Is.EqualTo(TstMainController.MessageStart + "1"));
        }

        [Test]
        public void SecondSettingsObjectOnTheSameFile()
        {
            var secondProvider = new SqliteSettingsProvider(dbPath, TstMainController.Levels, TstMainController.MessageStart,
                100, 5);
            Assert.That(secondProvider.PageSize, Is.EqualTo(100));
            ParsingTemplate template = secondProvider.ReadParsingTemplate();
            Assert.That(template.Name, Is.EqualTo("default"));
        }
    }
}