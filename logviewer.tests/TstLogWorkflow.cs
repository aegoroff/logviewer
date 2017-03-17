// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 17.03.2017
// © 2012-2017 Alexander Egorov

using System.ComponentModel;
using logviewer.logic;
using logviewer.logic.ui.main;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstLogWorkflow
    {
        private readonly Mock<IMainViewModel> viewModel;

        public TstLogWorkflow()
        {
            this.viewModel = new Mock<IMainViewModel>();
            var settings = new Mock<ISettingsProvider>();
            this.viewModel.SetupGet(_ => _.SettingsProvider).Returns(settings.Object);

            var model = new MainModel(this.viewModel.Object);
            var workflow = new LogWorkflow(model, this.viewModel.Object);
            workflow.Start();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void SetUseRegularExpressions_ChangeUseRegularExpressionsWhenFilterNotSet_UiControlsEnabledNeverGet(string filter)
        {
            // Arrange
            this.viewModel.SetupGet(v => v.MessageFilter).Returns(filter);
            this.viewModel.Raise(x => x.PropertyChanged += null, new PropertyChangedEventArgs(nameof(this.viewModel.Object.UseRegularExpressions)));

            // Act
            this.viewModel.SetupSet(x => x.UseRegularExpressions = true);

            // Assert
            this.viewModel.VerifyGet(x => x.UiControlsEnabled, Times.Never);
        }

        [Fact]
        public void SetUseRegularExpressions_ChangeUseRegularExpressionsWhenFilterSet_UiControlsEnabledGetOnce()
        {
            // Arrange
            this.viewModel.SetupGet(v => v.UiControlsEnabled).Returns(false);
            this.viewModel.SetupGet(v => v.MessageFilter).Returns("1");
            this.viewModel.Raise(x => x.PropertyChanged += null, new PropertyChangedEventArgs(nameof(this.viewModel.Object.UseRegularExpressions)));

            // Act
            this.viewModel.SetupSet(x => x.UseRegularExpressions = true);

            // Assert
            this.viewModel.VerifyGet(x => x.UiControlsEnabled, Times.Once);
        }

        [Theory]
        [InlineData("From")]
        [InlineData("To")]
        [InlineData("MinLevel")]
        [InlineData("MaxLevel")]
        [InlineData("SortingOrder")]
        public void PropertyChanged_ChangeAllFilterRelatedProperties_UiControlsEnabledGetOnce(string property)
        {
            // Arrange
            this.viewModel.SetupGet(v => v.UiControlsEnabled).Returns(false);

            // Act
            this.viewModel.Raise(x => x.PropertyChanged += null, new PropertyChangedEventArgs(property));

            // Assert
            this.viewModel.VerifyGet(x => x.UiControlsEnabled, Times.Once);
        }

        [Theory]
        [InlineData("LogSize")]
        [InlineData("LogEncoding")]
        [InlineData("LogStatistic")]
        [InlineData("ToDisplayMessages")]
        [InlineData("TotalMessages")]
        [InlineData("IsTextFilterFocused")]
        [InlineData("UiControlsEnabled")]
        public void PropertyChanged_ChangeAllNotFilterRelatedProperties_UiControlsEnabledNeverGet(string property)
        {
            // Arrange
            this.viewModel.SetupGet(v => v.UiControlsEnabled).Returns(false);

            // Act
            this.viewModel.Raise(x => x.PropertyChanged += null, new PropertyChangedEventArgs(property));

            // Assert
            this.viewModel.VerifyGet(x => x.UiControlsEnabled, Times.Never);
        }
    }
}