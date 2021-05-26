// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 11.01.2014
// © 2012-2018 Alexander Egorov

using System;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using logviewer.logic.models;
using logviewer.logic.storage;
using Xunit;

namespace logviewer.tests
{
    [Collection("SerialTests")]
    public class TstLocalDbSettingsProvider : IDisposable
    {
        private const int KeepLastNFilesCount = 2;

        private const int ParsingTemplatesCount = 11;

        private static readonly string messageStart = ParsingTemplate.Defaults.First().StartMessage;
        
        private static readonly string dbPath = Path.GetTempFileName();

        public TstLocalDbSettingsProvider()
        {
            this.provider = new LocalDbSettingsProvider(dbPath, 100, KeepLastNFilesCount);
        }

        public void Dispose()
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
        
        private readonly LocalDbSettingsProvider provider;

        [Fact]
        public void KeepLastNFiles_ReturnValue_ValueAsExpected()
        {
            // Arrange
            
            // Act
            var result = this.provider.KeepLastNFiles;
            
            // Assert
            result.Should().Be(KeepLastNFilesCount);
        }

        [Fact]
        public void PageSize_ReturnValue_ValueAsExpected()
        {
            // Arrange
            
            // Act
            var result = this.provider.PageSize;
            
            // Assert
            result.Should().Be(100);
        }

        [Fact]
        public void ReadParsingTemplate_TestDefaults_ResultsAsExpected()
        {
            // Arrange
            
            // Act
            var template = this.provider.ReadParsingTemplate();
            
            // Assert
            template.Index.Should().Be(0);
            template.StartMessage.Should().Be(messageStart);
        }

        [Fact]
        public void UpdateParsingTemplate_UpdatingTest_TemplateUpdated()
        {
            // Arrange
            var template = this.provider.ReadParsingTemplate();
            template.StartMessage += "1";

            // Act
            this.provider.UpdateParsingTemplate(template);

            // Assert
            var template1 = this.provider.ReadParsingTemplate();
            template1.StartMessage.Should().Be(messageStart + "1");
        }
        
        [Fact]
        public void UpdateParsingTemplate_WithFilter_TemplateUpdated()
        {
            // Arrange
            const string filter = "^#%{DATA}";
            var template = this.provider.ReadParsingTemplate();
            template.StartMessage += "1";
            template.Filter = filter;
            
            // Act
            this.provider.UpdateParsingTemplate(template);
            
            // Assert
            var template1 = this.provider.ReadParsingTemplate();
            template1.StartMessage.Should().Be(messageStart + "1");
            template1.Filter.Should().Be(filter);
        }

        [Fact]
        public void ReadParsingTemplate_SecondSettingsObjectOnTheSameFile_SameTemplateRead()
        {
            // Arrange
            var secondProvider = new LocalDbSettingsProvider(dbPath, 100, KeepLastNFilesCount);
            secondProvider.PageSize.Should().Be(100);
            
            // Act
            var template = secondProvider.ReadParsingTemplate();
            
            // Assert
            template.Name.Should().Be(ParsingTemplate.Defaults.First().Name);
        }

        [Fact]
        public void ReadParsingTemplateNames_ReadAllTemplates_TemplatesNumberAsExpected()
        {
            // Arrange
            
            // Act
            var list = this.provider.ReadParsingTemplateNames();
            
            // Assert
            list.Count.Should().Be(ParsingTemplatesCount);
        }
        
        [Fact]
        public void ReadAllParsingTemplates_ReadAllTemplates_TemplatesNumberAsExpected()
        {
            // Arrange
            
            // Act
            var list = this.provider.ReadAllParsingTemplates();
            
            // Assert
            list.Count.Should().Be(ParsingTemplatesCount);
        }

        [Fact]
        public void AutoRefreshOnFileChange_AutoRefreshTest_PropertyChanged()
        {
            // Arrange
            this.provider.AutoRefreshOnFileChange.Should().BeFalse();
            
            // Act
            this.provider.AutoRefreshOnFileChange = true;
            
            // Assert
            this.provider.AutoRefreshOnFileChange.Should().BeTrue();
        }
        
        [Fact]
        public void LastUpdateCheckTime_SettingPriperty_PropertySetCorrectly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var last = this.provider.LastUpdateCheckTime;
            last.Year.Should().Be(now.Year);
            last.Month.Should().Be(now.Month);
            last.Day.Should().Be(now.Day);
            last.Hour.Should().Be(now.Hour);
            last.Minute.Should().Be(now.Minute);

            var newValue = new DateTime(2014, 1, 10).ToUniversalTime();
            
            // Act
            this.provider.LastUpdateCheckTime = newValue;
            
            // Assert
            this.provider.LastUpdateCheckTime.Should().Be(newValue);
        }

        [Fact]
        public void InsertParsingTemplate_AddNewTemplate_TemplateAdded()
        {
            // Arrange
            var templates = this.provider.ReadParsingTemplateNames();
            templates.Count.Should().Be(ParsingTemplatesCount);
            var newTemplate = new ParsingTemplate
            {
                Index = templates.Count, 
                Name = "second", 
                StartMessage = "%{DATA}"
            };
            
            // Act
            this.provider.InsertParsingTemplate(newTemplate);
            
            // Asssert
            templates = this.provider.ReadParsingTemplateNames();
            templates.Count.Should().Be(ParsingTemplatesCount + 1);
            var template = this.provider.ReadParsingTemplate(newTemplate.Index);
            template.Index.Should().Be(newTemplate.Index);
            template.Name.Should().Be("second");
            template.StartMessage.Should().Be("%{DATA}");
        }
        
        [Fact]
        public void DeleteParsingTemplate_RemoveLastParsingTemplate_TemplateRemoved()
        {
            // Arrange
            var templates = this.provider.ReadParsingTemplateNames();
            templates.Count.Should().Be(ParsingTemplatesCount);
            
            // Act
            this.provider.DeleteParsingTemplate(ParsingTemplatesCount - 1);
            
            // Assert
            templates = this.provider.ReadParsingTemplateNames();
            templates.Count.Should().Be(ParsingTemplatesCount - 1);
        }
        
        [Fact]
        public void DeleteParsingTemplate_RemoveParsingTemplateFromMiddle_TemplateRemoved()
        {
            // Arrange
            var templates = this.provider.ReadParsingTemplateNames();
            templates.Count.Should().Be(ParsingTemplatesCount);
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
            templates = this.provider.ReadParsingTemplateNames();
            templates.Count.Should().Be(ParsingTemplatesCount + 2);
            
            // Act
            this.provider.DeleteParsingTemplate(newTemplate.Index);
            
            // Assert
            templates = this.provider.ReadParsingTemplateNames();
            templates.Count.Should().Be(ParsingTemplatesCount + 1);
            var template = this.provider.ReadParsingTemplate(templates.Count - 1);
            template.Name.Should().Be(newTemplate1.Name);
        }

        [Fact]
        public void GetUsingRecentFilesStore_NoRecentFiles_EmptyResult()
        {
            // Arrange
            
            // Act
            var files = this.provider.GetUsingRecentFilesStore(store => store.ReadItems());
            
            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        public void GetUsingRecentFilesStore_SaveAndReadRecentFilesOneFile_ResultAsExpected()
        {
            // Arrange
            const string f = "f1";
            this.provider.ExecuteUsingRecentFilesStore(store => store.Add(f));
            
            // Act
            var files = this.provider.GetUsingRecentFilesStore(store => store.ReadItems());
            
            // Assert
            files.Should().BeEquivalentTo(f);
        }

        [Fact]
        public void GetUsingRecentFilesStore_SaveAndReadRecentFilesManyFiles_ResultAsExpected()
        {
            // Arrange
            const string f1 = "f1";
            const string f2 = "f2";

            this.provider.ExecuteUsingRecentFilesStore(store => store.Add(f1));
            this.provider.ExecuteUsingRecentFilesStore(store => store.Add(f2));
            
            // Act
            var files = this.provider.GetUsingRecentFilesStore(store => store.ReadItems());
            
            // Assert
            files.Should().BeEquivalentTo(f1, f2);
        }

        [Fact]
        public void GetUsingRecentFilesStore_RecentFilesMoreThenLimit_LimitedCollectionReturned()
        {
            // Arrange
            const string f1 = "f1";
            const string f2 = "f2";
            const string f3 = "f3";

            this.provider.ExecuteUsingRecentFilesStore(store => store.Add(f1));
            Thread.Sleep(60);
            this.provider.ExecuteUsingRecentFilesStore(store => store.Add(f2));
            Thread.Sleep(60);
            this.provider.ExecuteUsingRecentFilesStore(store => store.Add(f3));
            
            // Act
            var files = this.provider.GetUsingRecentFilesStore(store => store.ReadItems());
            
            // Assert
            files.Should().BeEquivalentTo(f2, f3);
        }
    }
}
