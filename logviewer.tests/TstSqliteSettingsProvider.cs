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
        private static readonly string dbPath = Path.GetTempFileName();

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
        
        [Test]
        public void ReadParsingTemplate()
        {
            var provider = new SqliteSettingsProvider(dbPath, TstMainController.Levels, TstMainController.MessageStart, 100, 10);
            var template = provider.ReadParsingTemplate();
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
            var provider = new SqliteSettingsProvider(dbPath, TstMainController.Levels, TstMainController.MessageStart, 100, 10);
            var template = provider.ReadParsingTemplate();
            template.Trace += "1";
            template.Debug += "1";
            template.Info += "1";
            template.Warn += "1";
            template.Error += "1";
            template.Fatal += "1";
            template.StartMessage += "1";

            provider.UpdateParsingProfile(template);

            var template1 = provider.ReadParsingTemplate();

            Assert.That(template1.Trace, Is.EqualTo(TstMainController.Levels[0] + "1"));
            Assert.That(template1.Debug, Is.EqualTo(TstMainController.Levels[1] + "1"));
            Assert.That(template1.Info, Is.EqualTo(TstMainController.Levels[2] + "1"));
            Assert.That(template1.Warn, Is.EqualTo(TstMainController.Levels[3] + "1"));
            Assert.That(template1.Error, Is.EqualTo(TstMainController.Levels[4] + "1"));
            Assert.That(template1.Fatal, Is.EqualTo(TstMainController.Levels[5] + "1"));
            Assert.That(template.StartMessage, Is.EqualTo(TstMainController.MessageStart + "1"));
        }
    }
}