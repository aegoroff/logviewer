// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 23.02.2017
// © 2012-2018 Alexander Egorov

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using logviewer.engine;
using logviewer.logic;
using logviewer.logic.Annotations;
using logviewer.logic.models;
using logviewer.logic.storage;
using logviewer.logic.ui;
using logviewer.logic.ui.main;
using logviewer.tests.support;
using Moq;
using TechTalk.SpecFlow;

namespace logviewer.tests
{
    [Binding]
    [PublicAPI]
    public class SpecOpenLogFileSteps //-V3072
    {
        private const string MessageExamples =
            "2008-12-27 19:31:47,250 [4688] INFO \nmessage body 1\n2008-12-27 19:40:11,906 [5272] ERROR \nmessage body 2";

        private const string MessageTemplate =
            "2008-12-27 19:31:47,250 [4688] {0} \nmessage body {1}";

        private readonly MainModel model;
        private readonly Mock<IMainViewModel> viewModel;

        private readonly LogWorkflow workflow;
        private bool completed;

        private string path;
        private Mock<ISettingsProvider> settings;
        private bool waitResult;

        public SpecOpenLogFileSteps()
        {
            this.viewModel = new Mock<IMainViewModel>();
            this.settings = new Mock<ISettingsProvider>();
            var itemsProvider = new Mock<IItemsProvider<string>>();
            this.viewModel.SetupGet(_ => _.SettingsProvider).Returns(this.settings.Object);
            var logProvider = new LogProvider(null, this.settings.Object);
            this.viewModel.SetupGet(_ => _.Provider).Returns(logProvider);
            this.viewModel.SetupGet(_ => _.Datasource).Returns(new VirtualizingCollection<string>(itemsProvider.Object));
            this.viewModel.SetupGet(_ => _.GithubAccount).Returns("egoroff");
            this.viewModel.SetupGet(_ => _.GithubProject).Returns("logviewer");

            var template = ParsingTemplate(logic.models.ParsingTemplate.Defaults.First().StartMessage);
            this.settings.Setup(_ => _.ReadParsingTemplate()).Returns(template);
            this.settings.SetupGet(x => x.AutoRefreshOnFileChange).Returns(true);

            this.model = new MainModelForTest(this.viewModel.Object);
            this.model.ReadCompleted += this.OnReadCompleted;
            this.workflow = new LogWorkflow(this.model, this.viewModel.Object);
        }

        [Given(@"I have file ""(.*)"" on disk")]
        public void GivenIHaveFileOnDisk(string filePath)
        {
            this.path = filePath;
            this.viewModel.SetupGet(_ => _.LogPath).Returns(this.path);
        }

        [Given(@"The file contains (.*) messages with levels ""(.*)"" and ""(.*)""")]
        public void GivenTheFileContainsMessagesWithLevelsAnd(int count, string level1, string level2)
        {
            var content = CreateContent(count, level1, level2);
            File.WriteAllText(this.path, content);
        }

        [When(@"I press open with default filtering parameters")]
        public void WhenIPressOpenWithDefaultFilteringParameters()
        {
            this.Open(LogLevel.Trace, LogLevel.Fatal);
        }

        [When(@"I press reload")]
        public void WhenIPressReload()
        {
            this.workflow.Reload();
        }

        [When(@"I press open with min level ""(.*)"" and max level ""(.*)""")]
        public void WhenIPressOpenWithMinLevelAndMaxLevel(string min, string max)
        {
            Enum.TryParse(min, out LogLevel minLevel);

            Enum.TryParse(max, out LogLevel maxLevel);

            this.Open(minLevel, maxLevel);
        }

        [When(@"I press open with message text filter ""(.*)""")]
        public void WhenIPressOpenWithMessageTextFilter(string filter)
        {
            this.Open(LogLevel.Trace, LogLevel.Fatal, filter);
        }

        [When(@"I press open with message text filter ""(.*)"" and regular expression support enabled")]
        public void WhenIPressOpenWithMessageTextFilterAndRegularExpressionSupportEnabled(string filter)
        {
            this.Open(LogLevel.Trace, LogLevel.Fatal, filter, true);
        }

        [When(@"wait (.*) seconds")]
        public void WhenWaitSeconds(int seconds)
        {
            this.waitResult = SpinWait.SpinUntil(() => this.completed, TimeSpan.FromSeconds(seconds));
        }

        [When(@"freeze (.*) seconds")]
        public void WhenFreezeSeconds(int seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        [When(@"I start application with default filtering parameters")]
        public void WhenIStartApplicationWithDefaultFilteringParameters()
        {
            this.settings.SetupGet(x => x.OpenLastFile).Returns(true);

            this.settings.Setup(x => x.GetUsingRecentFilesStore(It.IsAny<Func<IStringCollectionStore, string>>())).Returns(this.path);

            this.viewModel.SetupGet(_ => _.MinLevel).Returns((int) LogLevel.Trace);
            this.viewModel.SetupGet(_ => _.MaxLevel).Returns((int) LogLevel.Fatal);
            this.viewModel.SetupGet(_ => _.From).Returns(DateTime.MinValue);
            this.viewModel.SetupGet(_ => _.To).Returns(DateTime.MaxValue);

            this.workflow.StartApplication();
        }

        [Then(@"the number of shown messages should be (.*)")]
        public void ThenTheNumberOfReadMessagesShouldBeInTheLogStore(int messagesCount)
        {
            this.waitResult.Should().BeTrue("Read should be completed in 2 second");
            this.viewModel.VerifySet(x => x.MessageCount = messagesCount);
        }

        [When(@"Add (.*) more messages with levels ""(.*)"" and ""(.*)"" into log")]
        public void WhenAddMoreMessagesWithLevelsAndIntoLog(int count, string level1, string level2)
        {
            var content = CreateContent(count, level1, level2);
            File.AppendAllText(this.path, content);
        }

        [AfterScenario("mainmodel")]
        public void AfterScenario()
        {
            try
            {
                this.workflow?.Close();
                this.workflow?.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Thread.Sleep(50);
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    if (File.Exists(this.path))
                    {
                        File.Delete(this.path);
                    }
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static string CreateContent(int count, string level1, string level2)
        {
            var sb = new StringBuilder();
            var levels = new[] { level1, level2 };
            for (var i = 0; i < count; i++)
            {
                sb.AppendFormat(MessageTemplate, levels[i % levels.Length], i + 1);
                sb.AppendLine();
            }

            var content = sb.ToString();
            return content;
        }

        private void Open(LogLevel minLevel, LogLevel maxLevel, string filter = null, bool useRegularExpressions = false)
        {
            this.viewModel.SetupGet(_ => _.MinLevel).Returns((int) minLevel);
            this.viewModel.SetupGet(_ => _.MaxLevel).Returns((int) maxLevel);
            this.viewModel.SetupGet(_ => _.From).Returns(DateTime.MinValue);
            this.viewModel.SetupGet(_ => _.To).Returns(DateTime.MaxValue);
            this.viewModel.SetupGet(_ => _.UseRegularExpressions).Returns(useRegularExpressions);
            this.viewModel.SetupGet(_ => _.MessageFilter).Returns(filter);
            this.workflow.Open(this.path);
        }

        private void OnReadCompleted(object sender, EventArgs e)
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
    }
}
