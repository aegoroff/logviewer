// Created by: egr
// Created at: 11.01.2014
// © 2012-2014 Alexander Egorov

using System;
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
            this.provider.UpdateParsingTemplate(template);
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
        public void ReadAllParsingTemplates()
        {
            var list = this.provider.ReadAllParsingTemplates();
            Assert.That(list.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AutoRefreshTest()
        {
            Assert.That(this.provider.AutoRefreshOnFileChange, Is.False);
            this.provider.AutoRefreshOnFileChange = true;
            Assert.That(this.provider.AutoRefreshOnFileChange, Is.True);
        }
        
        [Test]
        public void LastChekingUpdateTest()
        {
            Assert.That(this.provider.LastUpdateCheckTime, Is.EqualTo(DateTime.UtcNow).Within(5).Seconds);
            var newValue = new DateTime(2014, 1, 10).ToUniversalTime();
            this.provider.LastUpdateCheckTime = newValue;
            Assert.That(this.provider.LastUpdateCheckTime, Is.EqualTo(newValue));
        }

        [Test]
        public void InsertParsingTemplate()
        {
            var templates = this.provider.ReadParsingTemplateList();
            Assert.That(templates.Count, Is.EqualTo(1));
            var newTemplate = new ParsingTemplate
            {
                Index = templates.Count, 
                Name = "second", 
                StartMessage = "%{DATA}"
            };
            this.provider.InsertParsingTemplate(newTemplate);
            templates = this.provider.ReadParsingTemplateList();
            Assert.That(templates.Count, Is.EqualTo(2));
            var template = this.provider.ReadParsingTemplate(newTemplate.Index);
            Assert.That(template.Index, Is.EqualTo(newTemplate.Index));
            Assert.That(template.Name, Is.EqualTo("second"));
            Assert.That(template.StartMessage, Is.EqualTo("%{DATA}"));
        }
        
        [Test]
        public void RemoveParsingTemplate()
        {
            var templates = this.provider.ReadParsingTemplateList();
            Assert.That(templates.Count, Is.EqualTo(1));
            var newTemplate = new ParsingTemplate
            {
                Index = templates.Count, 
                Name = "second", 
                StartMessage = "^.+?$"
            };
            this.provider.InsertParsingTemplate(newTemplate);
            templates = this.provider.ReadParsingTemplateList();
            Assert.That(templates.Count, Is.EqualTo(2));
            this.provider.DeleteParsingTemplate(newTemplate.Index);
            templates = this.provider.ReadParsingTemplateList();
            Assert.That(templates.Count, Is.EqualTo(1));
        }
        
        [Test]
        public void RemoveParsingTemplateFromMiddle()
        {
            var templates = this.provider.ReadParsingTemplateList();
            Assert.That(templates.Count, Is.EqualTo(1));
            var newTemplate = new ParsingTemplate
            {
                Index = templates.Count, 
                Name = "second", 
                StartMessage = "^.+?$"
            };
            
            var newTemplate1 = new ParsingTemplate
            {
                Index = templates.Count + 1, 
                Name = "third", 
                StartMessage = "^.+?$"
            };
            this.provider.InsertParsingTemplate(newTemplate);
            this.provider.InsertParsingTemplate(newTemplate1);
            templates = this.provider.ReadParsingTemplateList();
            Assert.That(templates.Count, Is.EqualTo(3));
            this.provider.DeleteParsingTemplate(newTemplate.Index);
            templates = this.provider.ReadParsingTemplateList();
            Assert.That(templates.Count, Is.EqualTo(2));
            ParsingTemplate template = this.provider.ReadParsingTemplate(templates.Count - 1);
            Assert.That(template.Name, Is.EqualTo(newTemplate1.Name));
        }
    }
}