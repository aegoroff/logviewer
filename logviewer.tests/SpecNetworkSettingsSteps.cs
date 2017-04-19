using System;
using FluentAssertions;
using logviewer.logic.Annotations;
using logviewer.logic.storage;
using logviewer.logic.support;
using logviewer.logic.ui.network;
using Moq;
using TechTalk.SpecFlow;

namespace logviewer.tests
{
    [Binding]
    [PublicAPI]
    public class SpecNetworkSettingsSteps
    {
        private readonly Mock<ISimpleOptionsStore> settingsProvider;
        private readonly NetworkSettingsViewModel viewModel;
        private NetworkSettingsModel model;
        private readonly AsymCrypt crypt;

        public SpecNetworkSettingsSteps()
        {
            this.settingsProvider = new Mock<ISimpleOptionsStore>();
            this.viewModel = new NetworkSettingsViewModel();
            this.crypt = new AsymCrypt();
            this.crypt.GenerateKeys();
            this.settingsProvider.Setup(p => p.ReadStringOption("PrivateKey", It.IsAny<string>())).Returns(this.crypt.PrivateKey);
            this.settingsProvider.Setup(p => p.ReadStringOption("PublicKey", It.IsAny<string>())).Returns(this.crypt.PublicKey);
        }

        [Given(@"I have stored ""(.*)"" option ""(.*)""")]
        public void GivenIHaveStoredOption(string value, string option)
        {
            this.settingsProvider.Setup(p => p.ReadStringOption(option, It.IsAny<string>())).Returns(value);
        }
        
        [Given(@"I have stored (.*) option ""(.*)""")]
        public void GivenIHaveStoredOption(int value, string option)
        {
            this.settingsProvider.Setup(p => p.ReadIntegerOption(option, It.IsAny<int>())).Returns(value);
        }
        
        [Given(@"I have stored ""(.*)"" boolean option ""(.*)""")]
        public void GivenIHaveStoredBooleanOption(string value, string option)
        {
            this.settingsProvider.Setup(p => p.ReadBooleanOption(option, It.IsAny<bool>())).Returns(bool.Parse(value));
        }

        [Given(@"I have crypted and stored ""(.*)"" option ""(.*)""")]
        public void GivenIHaveCryptedAndStoredOption(string value, string option)
        {
            this.settingsProvider.Setup(p => p.ReadStringOption(option, It.IsAny<string>())).Returns(this.crypt.Encrypt(value));
        }

        [When(@"I open network settings")]
        public void WhenIOpenNetworkSettings()
        {
            this.model = new NetworkSettingsModel(this.viewModel, this.settingsProvider.Object);
            this.model.Initialize();
        }
        
        [When(@"Change mode to ""(.*)""")]
        public void WhenChangeModeTo(string mode)
        {
            ProxyMode newMode;
            Enum.TryParse(mode, out newMode);
            this.viewModel.IsNoUseProxy = newMode == ProxyMode.None;
            this.viewModel.IsUseAutoProxy = newMode == ProxyMode.AutoProxyDetection;
            this.viewModel.IsUseManualProxy = newMode == ProxyMode.Custom;
        }
        
        [Then(@"I see IsUseAutoProxy set to ""(.*)""")]
        public void ThenISeeIsUseAutoProxySetTo(string value)
        {
            this.viewModel.IsUseAutoProxy.Should().Be(bool.Parse(value));
        }
        
        [Then(@"IsNoUseProxy set to ""(.*)""")]
        public void ThenIsNoUseProxySetTo(string value)
        {
            this.viewModel.IsNoUseProxy.Should().Be(bool.Parse(value));
        }
        
        [Then(@"IsUseManualProxy set to ""(.*)""")]
        public void ThenIsUseManualProxySetTo(string value)
        {
            this.viewModel.IsUseManualProxy.Should().Be(bool.Parse(value));
        }
        
        [Then(@"Host set to ""(.*)""")]
        public void ThenHostSetTo(string value)
        {
            this.viewModel.Host.Should().Be(value);
        }

        [Then(@"Host set to null")]
        public void ThenHostSetToNull()
        {
            this.viewModel.Host.Should().BeNull();
        }

        [Then(@"Port set to (.*)")]
        public void ThenPortSetTo(int value)
        {
            this.viewModel.Port.Should().Be(value);
        }
        
        [Then(@"IsUseDefaultCredentials set to ""(.*)""")]
        public void ThenIsUseDefaultCredentialsSetTo(string value)
        {
            this.viewModel.IsUseDefaultCredentials.Should().Be(bool.Parse(value));
        }

        [Then(@"Login set to ""(.*)""")]
        public void ThenLoginSetTo(string value)
        {
            this.viewModel.UserName.Should().Be(value);
        }

        [Then(@"Password set to ""(.*)""")]
        public void ThenPasswordSetTo(string value)
        {
            this.viewModel.Password.Should().Be(value);
        }

        [Then(@"Domain set to ""(.*)""")]
        public void ThenDomainSetTo(string value)
        {
            this.viewModel.Domain.Should().Be(value);
        }

    }
}
