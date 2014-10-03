// Created by: egr
// Created at: 11.01.2014
// © 2012-2014 Alexander Egorov

using System.IO;
using System.Linq;
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
            this.provider = new SqliteSettingsProvider(dbPath, TstMainController.MessageStart, 100, 5);
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
            Assert.That(template.StartMessage, Is.EqualTo(TstMainController.MessageStart));
        }

        [Test]
        public void UpdateParsingTemplate()
        {
            ParsingTemplate template = this.provider.ReadParsingTemplate();
            template.StartMessage += "1";
            this.provider.UpdateParsingProfile(template);
            ParsingTemplate template1 = this.provider.ReadParsingTemplate();
            Assert.That(template1.StartMessage, Is.EqualTo(TstMainController.MessageStart + "1"));
        }

        [Test]
        public void SecondSettingsObjectOnTheSameFile()
        {
            var secondProvider = new SqliteSettingsProvider(dbPath, TstMainController.MessageStart, 100, 5);
            Assert.That(secondProvider.PageSize, Is.EqualTo(100));
            ParsingTemplate template = secondProvider.ReadParsingTemplate();
            Assert.That(template.Name, Is.EqualTo("default"));
        }

        [Test]
        public void ReadParsingTemplateList()
        {
            var list = this.provider.ReadParsingTemplateList();
            Assert.That(list.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AutoRefreshTest()
        {
            Assert.That(this.provider.AutoRefreshOnFileChange, Is.False);
            this.provider.AutoRefreshOnFileChange = true;
            Assert.That(this.provider.AutoRefreshOnFileChange, Is.True);
        }
    }
}