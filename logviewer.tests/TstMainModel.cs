// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 18.11.2015
// © 2012-2017 Alexander Egorov

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using logviewer.engine;
using logviewer.logic;
using logviewer.logic.models;
using logviewer.logic.storage;
using logviewer.logic.ui;
using logviewer.logic.ui.main;
using Moq;
using Net.Sgoliver.NRtfTree.Util;
using Xunit;

namespace logviewer.tests
{
    [Collection("SerialTests")]
    public class TstMainModel : IDisposable
    {
        private const string f1 = "1";
        private const string f2 = "2";
        private const string f3 = "3";
        private const string SettingsDb = "test.db";

        #region Setup/Teardown

        public TstMainModel()
        {
            CleanupTestFiles();
            this.viewModel = new Mock<IMainViewModel>();
            this.settings = new Mock<ISettingsProvider>();
            var itemsProvider = new Mock<IItemsProvider<string>>();
            this.viewModel.SetupGet(_ => _.SettingsProvider).Returns(this.settings.Object);
            this.viewModel.SetupGet(_ => _.Provider).Returns(new LogProvider(null, this.settings.Object));
            this.viewModel.SetupGet(_ => _.Datasource).Returns(new VirtualizingCollection<string>(itemsProvider.Object));
            this.viewModel.SetupGet(_ => _.GithubAccount).Returns("egoroff");
            this.viewModel.SetupGet(_ => _.GithubProject).Returns("logviewer");

            var template = ParsingTemplate(logic.models.ParsingTemplate.Defaults.First().StartMessage);
            this.settings.Setup(x => x.ReadParsingTemplate()).Returns(template);

            this.model = new MainModel(this.viewModel.Object);
            this.model.ReadCompleted += this.OnReadCompleted;
        }

        private bool completed;

        void OnReadCompleted(object sender, EventArgs e)
        {
            this.completed = true;
        }

        private static ParsingTemplate ParsingTemplate(string startMessage)
        {
            return new ParsingTemplate
            {
                Index = 0,
                StartMessage = startMessage
            };
        }

        public void Dispose()
        {
            Cleanup(TestPath, RecentPath, FullPathToTestDb);
            CleanupTestFiles();
            this.model.Dispose();
        }

        private static string FullPathToTestDb => Path.Combine(LocalDbSettingsProvider.ApplicationFolder, SettingsDb);

        private static void CleanupTestFiles()
        {
            Cleanup(f1, f2, f3);
        }
        
        private static void Cleanup(params string[] files)
        {
            foreach (var file in files.Where(File.Exists))
            {
                for (var i = 0; i < 5; i++)
                {
                    try
                    {
                        File.Delete(file);
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        private void WaitReadingComplete()
        {
            var result = SpinWait.SpinUntil(() => this.completed, TimeSpan.FromSeconds(2));
            result.Should().BeTrue("Wait expired");
        }

        private void ReadLogExpectations()
        {
            this.viewModel.SetupGet(v => v.LogPath).Returns(TestPath); // 1+

            this.settings.Setup(_ => _.FormatHead(It.IsAny<LogLevel>())).Returns(new RtfCharFormat { Font = "Courier New" });
            this.settings.Setup(_ => _.FormatBody(It.IsAny<LogLevel>())).Returns(new RtfCharFormat { Font = "Courier New" });
        }

        #endregion

        private const string TestPath = "f";
        private const string RecentPath = "r";
        private readonly Mock<IMainViewModel> viewModel;
        private readonly Mock<ISettingsProvider> settings;
        private readonly MainModel model;

        private const string MessageExamples =
            "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1\n2008-12-27 19:40:11,906 [5272] ERROR \nmessage body 2";

        internal static string MessageStart = logic.models.ParsingTemplate.Defaults.First().StartMessage;

        [Fact]
        public void AllFilters()
        {
            this.ReadLogExpectations();
            
            File.WriteAllText(TestPath,
                MessageExamples + "\n2008-12-27 19:42:11,906 [5272] WARN \n2008-12-27 19:42:11,906 [5555] WARN ");
            this.viewModel.SetupGet(v => v.MinLevel).Returns((int)LogLevel.Warn);
            this.viewModel.SetupGet(v => v.MaxLevel).Returns((int)LogLevel.Warn);
            this.viewModel.SetupGet(v => v.From).Returns(DateTime.MinValue);
            this.viewModel.SetupGet(v => v.To).Returns(DateTime.MaxValue);
            this.viewModel.SetupGet(v => v.MessageFilter).Returns("5555");
            this.viewModel.SetupGet(v => v.UseRegularExpressions).Returns(false);
            this.viewModel.SetupGet(v => v.UiControlsEnabled).Returns(true);
            this.viewModel.SetupSet(v => v.UiControlsEnabled = false);

            this.model.UpdateMatcherAndRefreshLog(true);
            this.WaitReadingComplete();
            this.model.Store.CountMessages().Should().Be(4);
        }

        /*
        public static IEnumerable<object[]> MaxDates => new[]
        {
            new object[] { new DateTime(2008, 12, 27, 19, 32, 0, DateTimeKind.Utc), 1 },
            new object[] { new DateTime(2008, 12, 27, 19, 52, 0, DateTimeKind.Utc), 2 },
            new object[] { new DateTime(2008, 12, 27, 19, 12, 0, DateTimeKind.Utc), 0 }
        };

        public static IEnumerable<object[]> MinDates => new[]
        {
            new object[] { new DateTime(2008, 12, 27, 19, 32, 0, DateTimeKind.Utc), 1 },
            new object[] { new DateTime(2008, 12, 27, 19, 52, 0, DateTimeKind.Utc), 0 },
            new object[] { new DateTime(2008, 12, 27, 19, 12, 0, DateTimeKind.Utc), 2 }
        };

        [Fact]
        public void ExportRtfFail()
        {
            this.viewModel.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.viewModel.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            this.viewModel.Setup(v => v.OpenExport(TestPath + ".rtf")).Returns(false); // 1
            this.model.ExportToRtf();
            this.viewModel.Verify();
        }

        [Fact]
        public void ExportRtfSuccess()
        {
            this.viewModel.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.viewModel.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            this.viewModel.Setup(v => v.OpenExport(TestPath + ".rtf")).Returns(true); // 1
            this.viewModel.Setup(v => v.SaveRtf()); // 1
            this.model.ExportToRtf();
            this.viewModel.Verify();
        }

        [Fact]
        public void OpenLogFileCanceled()
        {
            this.viewModel.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.viewModel.Setup(v => v.OpenLogFile()).Returns(false); // 1
            this.model.OpenLogFile();
        }

        [Fact]
        public void ReadEmptyFile()
        {
            this.viewModel.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.viewModel.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            File.Create(TestPath).Dispose();
            Assert.Throws<ArgumentException>(() => this.model.StartReadLog());
        }

        [Fact]
        public void ReadEmptyWhenMinGreaterThenMax()
        {
            this.viewModel.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1

            this.model.MinFilter((int)LogLevel.Error);
            this.model.MaxFilter((int)LogLevel.Info);
            File.Create(TestPath).Dispose();
            Assert.Throws<ArgumentException>(() => this.model.StartReadLog());
        }

        [Fact]
        public void ReadFromBadPath()
        {
            this.viewModel.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1

            this.viewModel.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            this.viewModel.SetupGet(v => v.LogPath).Returns(string.Empty); // 0
            Assert.Throws<ArgumentException>(() => this.model.StartReadLog());
        }

        [Fact]
        public void ReadNormalBigLog()
        {
            this.ReadLogExpectations();

            var sb = new StringBuilder();
            for (var i = 0; i < 1000; i++)
            {
                sb.AppendLine(MessageExamples);
            }
            this.viewModel.SetupGet(v => v.LogPath).Returns(TestPath); // 1+
            this.viewModel.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 2
            this.viewModel.Setup(v => v.SetLoadedFileCapltion(TestPath)); // 1
            File.WriteAllText(TestPath, sb.ToString());
            this.model.StartReadLog();
            this.WaitReadingComplete();
            this.model.MessagesCount.Should().Be(2000);
        }

        void ReadNormalLogInternal(Encoding encoding)
        {
            this.ReadLogExpectations();

            File.WriteAllText(TestPath, MessageExamples, encoding);
            this.model.StartReadLog();
            this.WaitReadingComplete();
            this.model.MessagesCount.Should().Be(2);
        }

        [Fact]
        public void ReadNormalLog()
        {
            this.ReadNormalLogInternal(Encoding.UTF8);
        }

        [Fact]
        public void ReadNotEmptyLogWithoutMessagesMatch()
        {
            this.ReadLogExpectations();

            File.WriteAllText(TestPath, "test log");
            this.model.StartReadLog();
            this.WaitReadingComplete();
            this.model.MessagesCount.Should().Be(1);
        }
        
        [Theory]
        [InlineData(MessageExamples)]
        [InlineData("2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1\n2008-12-27T19:40:11,906Z+03 [5272] ERROR \nmessage body 2")]
        public void LogLevelParsing(string examples)
        {
            this.ReadLogExpectations();

            File.WriteAllText(TestPath, examples);
            this.model.MinFilter((int)LogLevel.Info);
            this.model.StartReadLog();
            this.WaitReadingComplete();
            this.model.MessagesCount.Should().Be(2);
        }

        [Theory]
        [InlineData("test", false, true)]
        [InlineData("t[", false, true)]
        [InlineData("t[", true, false)]
        [InlineData("t[1]", true, true)]
        [InlineData(null, true, true)]
        [InlineData(null, false, true)]
        public void FilterValidation(string filter, bool useRegex, bool result)
        {
            this.viewModel.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            MainController.IsValidFilter(filter, useRegex).Should().Be(result);
        }

        [Fact]
        public void StartReadingWithinDelay()
        {
            this.settings.SetupSet(_ => _.MessageFilter = "f"); // 2
            this.settings.SetupGet(_ => _.FullPathToDatabase).Returns(FullPathToTestDb); // any

            this.viewModel.Setup(v => v.AddFilterItems(It.IsAny<string[]>())); // any
            this.viewModel.Setup(v => v.SetLoadedFileCapltion(It.IsAny<string>()));  // any
            this.viewModel.Setup(v => v.StartReading()); // Any
            this.viewModel.Setup(v => v.SetProgress(It.IsAny<LoadProgress>())); // Any
            this.viewModel.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.viewModel.Setup(v => v.StartReading()); // 0
            this.model.StartReadingCachedLog("f", false);
            this.model.StartReadingCachedLog("f", false);
        }
        
        [Fact]
        public void StartReadingOutsideDelay()
        {
            this.settings.SetupSet(_ => _.MessageFilter = "f"); // 2
            this.settings.SetupGet(_ => _.FullPathToDatabase).Returns(FullPathToTestDb); // any
            this.viewModel.Setup(v => v.AddFilterItems(It.IsAny<string[]>())); // any
            this.viewModel.Setup(v => v.SetLoadedFileCapltion(It.IsAny<string>()));  // any
            this.viewModel.Setup(v => v.StartReading()); // Any
            this.viewModel.Setup(v => v.SetProgress(It.IsAny<LoadProgress>())); // Any
            this.viewModel.Setup(v => v.SetLogProgressCustomText(It.IsAny<string>())); // 1
            this.viewModel.Setup(v => v.StartReading()); // 1
            this.model.StartReadingCachedLog("f", false);
            Thread.Sleep(TimeSpan.FromMilliseconds(300));
            this.model.PendingStart.Should().BeFalse();
        }
        */
    }
}