// Created by: egr
// Created at: 11.01.2014
// © 2012-2014 Alexander Egorov

using System;
using System.IO;
using System.Linq;
using logviewer.core;
using Xunit;

namespace logviewer.tests
{
    public class TstSqliteSettingsProvider : IDisposable
    {
        public TstSqliteSettingsProvider()
        {
            this.provider = new SqliteSettingsProvider(dbPath, 100, 5);
        }

        public void Dispose()
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }

        private static readonly string dbPath = Path.GetTempFileName();
        private readonly SqliteSettingsProvider provider;

        [Fact]
        public void KeepLastNFiles()
        {
            Assert.Equal(5, this.provider.KeepLastNFiles);
        }

        [Fact]
        public void PageSize()
        {
            Assert.Equal(100, this.provider.PageSize);
        }

        [Fact]
        public void ReadParsingTemplate()
        {
            ParsingTemplate template = this.provider.ReadParsingTemplate();
            Assert.Equal(0, template.Index);
            Assert.Equal(TstMainController.MessageStart, template.StartMessage);
        }

        [Fact]
        public void UpdateParsingTemplate()
        {
            ParsingTemplate template = this.provider.ReadParsingTemplate();
            template.StartMessage += "1";
            this.provider.UpdateParsingTemplate(template);
            ParsingTemplate template1 = this.provider.ReadParsingTemplate();
            Assert.Equal(TstMainController.MessageStart + "1", template1.StartMessage);
        }
        
        [Fact]
        public void UpdateParsingTemplateWithFilter()
        {
            const string filter = "^#%{DATA}";
            ParsingTemplate template = this.provider.ReadParsingTemplate();
            template.StartMessage += "1";
            template.Filter = filter;
            this.provider.UpdateParsingTemplate(template);
            ParsingTemplate template1 = this.provider.ReadParsingTemplate();
            Assert.Equal(TstMainController.MessageStart + "1", template1.StartMessage);
            Assert.Equal(filter, template1.Filter);
        }

        [Fact]
        public void SecondSettingsObjectOnTheSameFile()
        {
            var secondProvider = new SqliteSettingsProvider(dbPath, 100, 5);
            Assert.Equal(100, secondProvider.PageSize);
            ParsingTemplate template = secondProvider.ReadParsingTemplate();
            Assert.Equal(ParsingTemplate.Defaults.First().Name, template.Name);
        }

        [Fact]
        public void ReadParsingTemplateList()
        {
            var list = this.provider.ReadParsingTemplateList();
            Assert.Equal(9, list.Count());
        }
        
        [Fact]
        public void ReadAllParsingTemplates()
        {
            var list = this.provider.ReadAllParsingTemplates();
            Assert.Equal(9, list.Count());
        }

        [Fact]
        public void AutoRefreshTest()
        {
            Assert.False(this.provider.AutoRefreshOnFileChange);
            this.provider.AutoRefreshOnFileChange = true;
            Assert.True(this.provider.AutoRefreshOnFileChange);
        }
        
        [Fact]
        public void LastChekingUpdateTest()
        {
            Assert.Equal(DateTime.UtcNow, this.provider.LastUpdateCheckTime);
            var newValue = new DateTime(2014, 1, 10).ToUniversalTime();
            this.provider.LastUpdateCheckTime = newValue;
            Assert.Equal(newValue, this.provider.LastUpdateCheckTime);
        }

        [Fact]
        public void InsertParsingTemplate()
        {
            var templates = this.provider.ReadParsingTemplateList();
            Assert.Equal(9, templates.Count);
            var newTemplate = new ParsingTemplate
            {
                Index = templates.Count, 
                Name = "second", 
                StartMessage = "%{DATA}"
            };
            this.provider.InsertParsingTemplate(newTemplate);
            templates = this.provider.ReadParsingTemplateList();
            Assert.Equal(10, templates.Count);
            var template = this.provider.ReadParsingTemplate(newTemplate.Index);
            Assert.Equal(newTemplate.Index, template.Index);
            Assert.Equal("second", template.Name);
            Assert.Equal("%{DATA}", template.StartMessage);
        }
        
        [Fact]
        public void RemoveParsingTemplate()
        {
            var templates = this.provider.ReadParsingTemplateList();
            Assert.Equal(9, templates.Count);
            this.provider.DeleteParsingTemplate(8);
            templates = this.provider.ReadParsingTemplateList();
            Assert.Equal(8, templates.Count);
        }
        
        [Fact]
        public void RemoveParsingTemplateFromMiddle()
        {
            var templates = this.provider.ReadParsingTemplateList();
            Assert.Equal(9, templates.Count);
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
            Assert.Equal(11, templates.Count);
            this.provider.DeleteParsingTemplate(newTemplate.Index);
            templates = this.provider.ReadParsingTemplateList();
            Assert.Equal(10, templates.Count);
            ParsingTemplate template = this.provider.ReadParsingTemplate(templates.Count - 1);
            Assert.Equal(newTemplate1.Name, template.Name);
        }
    }
}