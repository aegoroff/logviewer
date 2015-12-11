// Created by: egr
// Created at: 11.01.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using logviewer.logic;
using Xunit;

namespace logviewer.tests
{
    [Collection("SerialTests")]
    public class TstSqliteSettingsProvider : IDisposable
    {
        private const int KeepLastNFilesCount = 2;

        public TstSqliteSettingsProvider()
        {
            this.provider = new SqliteSettingsProvider(dbPath, 100, KeepLastNFilesCount);
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
            this.provider.KeepLastNFiles.Should().Be(KeepLastNFilesCount);
        }

        [Fact]
        public void PageSize()
        {
            this.provider.PageSize.Should().Be(100);
        }

        [Fact]
        public void ReadParsingTemplate()
        {
            var template = this.provider.ReadParsingTemplate();
            template.Index.Should().Be(0);
            template.StartMessage.Should().Be(TstMainController.MessageStart);
        }

        [Fact]
        public void UpdateParsingTemplate()
        {
            var template = this.provider.ReadParsingTemplate();
            template.StartMessage += "1";
            this.provider.UpdateParsingTemplate(template);
            var template1 = this.provider.ReadParsingTemplate();
            template1.StartMessage.Should().Be(TstMainController.MessageStart + "1");
        }
        
        [Fact]
        public void UpdateParsingTemplateWithFilter()
        {
            const string filter = "^#%{DATA}";
            var template = this.provider.ReadParsingTemplate();
            template.StartMessage += "1";
            template.Filter = filter;
            this.provider.UpdateParsingTemplate(template);
            var template1 = this.provider.ReadParsingTemplate();
            template1.StartMessage.Should().Be(TstMainController.MessageStart + "1");
            template1.Filter.Should().Be(filter);
        }

        [Fact]
        public void SecondSettingsObjectOnTheSameFile()
        {
            var secondProvider = new SqliteSettingsProvider(dbPath, 100, KeepLastNFilesCount);
            secondProvider.PageSize.Should().Be(100);
            var template = secondProvider.ReadParsingTemplate();
            template.Name.Should().Be(ParsingTemplate.Defaults.First().Name);
        }

        [Fact]
        public void ReadParsingTemplateList()
        {
            var list = this.provider.ReadParsingTemplateList();
            list.Count.Should().Be(9);
        }
        
        [Fact]
        public void ReadAllParsingTemplates()
        {
            var list = this.provider.ReadAllParsingTemplates();
            list.Count.Should().Be(9);
        }

        [Fact]
        public void AutoRefreshTest()
        {
            this.provider.AutoRefreshOnFileChange.Should().BeFalse();
            this.provider.AutoRefreshOnFileChange = true;
            this.provider.AutoRefreshOnFileChange.Should().BeTrue();
        }
        
        [Fact]
        public void LastChekingUpdateTest()
        {
            var now = DateTime.UtcNow;
            var last = this.provider.LastUpdateCheckTime;
            last.Year.Should().Be(now.Year);
            last.Month.Should().Be(now.Month);
            last.Day.Should().Be(now.Day);
            last.Hour.Should().Be(now.Hour);
            last.Minute.Should().Be(now.Minute);

            var newValue = new DateTime(2014, 1, 10).ToUniversalTime();
            this.provider.LastUpdateCheckTime = newValue;
            this.provider.LastUpdateCheckTime.Should().Be(newValue);
        }

        [Fact]
        public void InsertParsingTemplate()
        {
            var templates = this.provider.ReadParsingTemplateList();
            templates.Count.Should().Be(9);
            var newTemplate = new ParsingTemplate
            {
                Index = templates.Count, 
                Name = "second", 
                StartMessage = "%{DATA}"
            };
            this.provider.InsertParsingTemplate(newTemplate);
            templates = this.provider.ReadParsingTemplateList();
            templates.Count.Should().Be(10);
            var template = this.provider.ReadParsingTemplate(newTemplate.Index);
            template.Index.Should().Be(newTemplate.Index);
            template.Name.Should().Be("second");
            template.StartMessage.Should().Be("%{DATA}");
        }
        
        [Fact]
        public void RemoveParsingTemplate()
        {
            var templates = this.provider.ReadParsingTemplateList();
            templates.Count.Should().Be(9);
            this.provider.DeleteParsingTemplate(8);
            templates = this.provider.ReadParsingTemplateList();
            templates.Count.Should().Be(8);
        }
        
        [Fact]
        public void RemoveParsingTemplateFromMiddle()
        {
            var templates = this.provider.ReadParsingTemplateList();
            templates.Count.Should().Be(9);
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
            templates.Count.Should().Be(11);
            this.provider.DeleteParsingTemplate(newTemplate.Index);
            templates = this.provider.ReadParsingTemplateList();
            templates.Count.Should().Be(10);
            var template = this.provider.ReadParsingTemplate(templates.Count - 1);
            template.Name.Should().Be(newTemplate1.Name);
        }

        [Fact]
        public void ReadRecentFilesEmpty()
        {
            IEnumerable<string> files = null;
            this.provider.UseRecentFilesStore(store => files = store.ReadItems());
            files.Should().BeEmpty();
        }

        [Fact]
        public void SaveAndReadRecentFilesOneFile()
        {
            const string f = "f1";
            IEnumerable<string> files = null;

            this.provider.UseRecentFilesStore(store => store.Add(f));
            this.provider.UseRecentFilesStore(store => files = store.ReadItems());
            files.Should().BeEquivalentTo(f);
        }

        [Fact]
        public void SaveAndReadRecentFilesManyFiles()
        {
            const string f1 = "f1";
            const string f2 = "f2";
            IEnumerable<string> files = null;

            this.provider.UseRecentFilesStore(store => store.Add(f1));
            this.provider.UseRecentFilesStore(store => store.Add(f2));
            this.provider.UseRecentFilesStore(store => files = store.ReadItems());
            files.Should().BeEquivalentTo(f1, f2);
        }

        [Fact]
        public void RecentFilesMoreThenLimit()
        {
            const string f1 = "f1";
            const string f2 = "f2";
            const string f3 = "f3";
            IEnumerable<string> files = null;

            this.provider.UseRecentFilesStore(store => store.Add(f1));
            Thread.Sleep(60);
            this.provider.UseRecentFilesStore(store => store.Add(f2));
            Thread.Sleep(60);
            this.provider.UseRecentFilesStore(store => store.Add(f3));
            this.provider.UseRecentFilesStore(store => files = store.ReadItems());
            files.Should().BeEquivalentTo(f2, f3);
        }
    }
}