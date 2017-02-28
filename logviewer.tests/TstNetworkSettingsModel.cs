// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 05.09.2007
// © 2007-2008 Alexander Egorov

using logviewer.logic;
using logviewer.logic.support;
using logviewer.logic.ui.network;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstNetworkSettingsModel
    {
        private const string HostValue = "h";
        private const string LoginValue = "l";
        private const string PasswordValue = "p";
        private const string DomainValue = "d";
        private const int PortValue = 8080;
        private readonly Mock<IOptionsProvider> provider;
        private readonly Mock<INetworkSettingsViewModel> viewModel;

        public TstNetworkSettingsModel()
        {
            this.provider = new Mock<IOptionsProvider>();
            this.viewModel = new Mock<INetworkSettingsViewModel>();
        }

        [Fact]
        public void Write_CustomProxyWithDefaultCredentials_CustomCredentialsNotAffected()
        {
            // Arrange
            this.viewModel.SetupGet(v => v.ProxyMode).Returns(ProxyMode.Custom);
            this.viewModel.SetupGet(v => v.Host).Returns("host");
            this.viewModel.SetupGet(v => v.Port).Returns(1234);
            this.viewModel.SetupGet(v => v.IsUseManualProxy).Returns(true);
            this.viewModel.SetupGet(v => v.IsUseAutoProxy).Returns(false);
            this.viewModel.SetupGet(v => v.IsNoUseProxy).Returns(false);
            this.viewModel.SetupGet(v => v.IsUseDefaultCredentials).Returns(true);
            this.viewModel.SetupGet(v => v.UserName).Returns("l2");
            this.viewModel.SetupGet(v => v.Password).Returns("p2");
            this.viewModel.SetupGet(v => v.Domain).Returns("d2");

            var model = new NetworkSettingsModel(this.viewModel.Object, this.provider.Object);

            // Act
            model.Write(string.Empty);

            // Assert
            this.provider.Verify(p => p.UpdateIntegerOption("ProxyMode", It.IsAny<int>()), Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Host", "host"), Times.Once);
            this.provider.Verify(p => p.UpdateIntegerOption("Port", 1234), Times.Once);

            this.provider.Verify(p => p.UpdateStringOption("Login", It.IsAny<string>()), Times.Never);
            this.provider.Verify(p => p.UpdateStringOption("Password", It.IsAny<string>()), Times.Never);
            this.provider.Verify(p => p.UpdateStringOption("Domain", It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(null, 8080)]
        [InlineData("", 8080)]
        [InlineData("  ", 8080)]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        [InlineData("  ", 0)]
        public void Write_CustomProxyBadProxyData_ProxySettingsNotSet(string host, int port)
        {
            // Arrange
            this.viewModel.SetupGet(v => v.ProxyMode).Returns(ProxyMode.Custom);
            this.viewModel.SetupGet(v => v.Host).Returns(host);
            this.viewModel.SetupGet(v => v.Port).Returns(port);
            this.viewModel.SetupGet(v => v.IsUseManualProxy).Returns(true);
            this.viewModel.SetupGet(v => v.IsUseAutoProxy).Returns(false);
            this.viewModel.SetupGet(v => v.IsNoUseProxy).Returns(false);
            this.viewModel.SetupGet(v => v.IsUseDefaultCredentials).Returns(true);
            this.viewModel.SetupGet(v => v.UserName).Returns("l2");
            this.viewModel.SetupGet(v => v.Password).Returns("p2");
            this.viewModel.SetupGet(v => v.Domain).Returns("d2");

            var model = new NetworkSettingsModel(this.viewModel.Object, this.provider.Object);

            // Act
            model.Write(string.Empty);

            // Assert
            this.provider.Verify(p => p.UpdateIntegerOption("ProxyMode", 2), Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Host", It.IsAny<string>()), Times.Never);
            this.provider.Verify(p => p.UpdateIntegerOption("Port", It.IsAny<int>()), Times.Never);

            this.provider.Verify(p => p.UpdateStringOption("Login", It.IsAny<string>()), Times.Never);
            this.provider.Verify(p => p.UpdateStringOption("Password", It.IsAny<string>()), Times.Never);
            this.provider.Verify(p => p.UpdateStringOption("Domain", It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Write_NoProxy_UseProxyFalseHostPortNotAffected()
        {
            // Arrange
            this.viewModel.SetupGet(v => v.ProxyMode).Returns(ProxyMode.None);
            this.viewModel.SetupGet(v => v.Host).Returns("host1");
            this.viewModel.SetupGet(v => v.Port).Returns(12341);
            this.viewModel.SetupGet(v => v.IsUseDefaultCredentials).Returns(false);
            this.viewModel.SetupGet(v => v.IsNoUseProxy).Returns(true);
            this.viewModel.SetupGet(v => v.IsUseManualProxy).Returns(false);
            this.viewModel.SetupGet(v => v.IsUseAutoProxy).Returns(false);

            var model = new NetworkSettingsModel(this.viewModel.Object, this.provider.Object);

            // Act
            model.Write(string.Empty);

            // Assert
            this.provider.Verify(p => p.UpdateIntegerOption("ProxyMode", 0), Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Host", It.IsAny<string>()), Times.Never);
            this.provider.Verify(p => p.UpdateIntegerOption("Port", It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Write_CustomModeWithCredentials_AllSet()
        {
            this.viewModel.SetupGet(v => v.ProxyMode).Returns(ProxyMode.Custom);
            this.viewModel.SetupGet(v => v.Host).Returns("host2");
            this.viewModel.SetupGet(v => v.Port).Returns(123411);
            this.viewModel.SetupGet(v => v.IsUseManualProxy).Returns(true);
            this.viewModel.SetupGet(v => v.IsUseAutoProxy).Returns(false);
            this.viewModel.SetupGet(v => v.IsNoUseProxy).Returns(false);
            this.viewModel.SetupGet(v => v.IsUseDefaultCredentials).Returns(false);
            this.viewModel.SetupGet(v => v.UserName).Returns("l1");
            this.viewModel.SetupGet(v => v.Password).Returns("p1");
            this.viewModel.SetupGet(v => v.Domain).Returns("d1");

            var model = new NetworkSettingsModel(this.viewModel.Object, this.provider.Object);

            // Act
            model.Write("p2");

            // Assert
            this.provider.Verify(p => p.UpdateIntegerOption("ProxyMode", 2), Times.Once);
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseDefaultCredentials", false), Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Host", "host2"), Times.Once);
            this.provider.Verify(p => p.UpdateIntegerOption("Port", 123411), Times.Once);

            this.viewModel.VerifySet(p => p.Password = "p2", Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Login", "l1"), Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Password", "p1"), Times.Never); // Don't save plain passwords
            this.provider.Verify(p => p.UpdateStringOption("Password", It.IsAny<string>()), Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Domain", "d1"), Times.Once);
        }

        [Fact]
        public void Write_AutoProxy_OnlyProxyFlagsAffected()
        {
            // Arrange
            this.viewModel.SetupGet(v => v.ProxyMode).Returns(ProxyMode.AutoProxyDetection);
            this.viewModel.SetupGet(v => v.IsUseManualProxy).Returns(false);
            this.viewModel.SetupGet(v => v.IsNoUseProxy).Returns(false);
            this.viewModel.SetupGet(v => v.IsUseAutoProxy).Returns(true);

            var model = new NetworkSettingsModel(this.viewModel.Object, this.provider.Object);

            // Act
            model.Write(string.Empty);

            // Assert
            this.provider.Verify(p => p.UpdateIntegerOption("ProxyMode", 1), Times.Once);

            this.provider.Verify(p => p.UpdateStringOption("Host", It.IsAny<string>()), Times.Never);
            this.provider.Verify(p => p.UpdateIntegerOption("Port", It.IsAny<int>()), Times.Never);
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseDefaultCredentials", It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void Initialize_NoProxy_Triggered()
        {
            // Arrange
            this.viewModel.SetupGet(v => v.ProxyMode).Returns(ProxyMode.None);

            var model = new NetworkSettingsModel(this.viewModel.Object, this.provider.Object);

            // Act
            model.Initialize();

            // Assert
            this.viewModel.VerifySet(vm => vm.ProxyMode = ProxyMode.None, Times.Once);
            this.viewModel.VerifySet(vm => vm.Host = null, Times.Once);
            this.viewModel.VerifySet(vm => vm.Port = 0, Times.Once);
            this.viewModel.VerifySet(vm => vm.UserName = null, Times.Once);
            this.viewModel.VerifySet(vm => vm.Password = null, Times.Once);
            this.viewModel.VerifySet(vm => vm.Domain = null, Times.Once);
        }

        [Fact]
        public void Initialize_AutoProxy_Triggered()
        {
            // Arrange
            this.viewModel.SetupGet(v => v.ProxyMode).Returns(ProxyMode.AutoProxyDetection);

            var model = new NetworkSettingsModel(this.viewModel.Object, this.provider.Object);

            // Act
            model.Initialize();

            // Assert
            this.viewModel.VerifySet(vm => vm.ProxyMode = ProxyMode.AutoProxyDetection, Times.Once);
            this.viewModel.VerifySet(vm => vm.Host = null, Times.Once);
            this.viewModel.VerifySet(vm => vm.Port = 0, Times.Once);
            this.viewModel.VerifySet(vm => vm.UserName = null, Times.Once);
            this.viewModel.VerifySet(vm => vm.Password = null, Times.Once);
            this.viewModel.VerifySet(vm => vm.Domain = null, Times.Once);
        }

        [Fact]
        public void Initialize_CustomProxyUseDefaultCredentials_Triggered()
        {
            // Arrange
            this.viewModel.SetupGet(v => v.ProxyMode).Returns(ProxyMode.Custom);
            this.viewModel.SetupGet(v => v.IsUseDefaultCredentials).Returns(true);

            this.provider.Setup(p => p.ReadIntegerOption("ProxyMode", 1)).Returns((int) ProxyMode.Custom);
            this.provider.Setup(p => p.ReadBooleanOption("IsUseDefaultCredentials", true)).Returns(true);
            this.provider.Setup(p => p.ReadStringOption("Host", null)).Returns(HostValue);
            this.provider.Setup(p => p.ReadIntegerOption("Port", 0)).Returns(PortValue);
            

            var model = new NetworkSettingsModel(this.viewModel.Object, this.provider.Object);

            // Act
            model.Initialize();

            // Assert
            this.viewModel.VerifySet(vm => vm.ProxyMode = ProxyMode.Custom, Times.Once);
            this.viewModel.VerifySet(vm => vm.Host = HostValue, Times.Once);
            this.viewModel.VerifySet(vm => vm.Port = PortValue, Times.Once);
            this.viewModel.VerifySet(vm => vm.UserName = null, Times.Once);
            this.viewModel.VerifySet(vm => vm.Password = null, Times.Once);
            this.viewModel.VerifySet(vm => vm.Domain = null, Times.Once);
        }

        [Fact]
        public void Initialize_CustomProxyUseCustomCredentials_Triggered()
        {
            // Arrange
            var crypt = new AsymCrypt();
            crypt.GenerateKeys();
            this.provider.Setup(p => p.ReadStringOption("PrivateKey", null)).Returns(crypt.PrivateKey);
            this.provider.Setup(p => p.ReadStringOption("PublicKey", null)).Returns(crypt.PublicKey);

            this.viewModel.SetupGet(v => v.ProxyMode).Returns(ProxyMode.Custom);
            this.viewModel.SetupGet(v => v.IsUseDefaultCredentials).Returns(false);

            this.provider.Setup(p => p.ReadIntegerOption("ProxyMode", 1)).Returns((int) ProxyMode.Custom);
            this.provider.Setup(p => p.ReadBooleanOption("IsUseDefaultCredentials", true)).Returns(false);
            this.provider.Setup(p => p.ReadStringOption("Host", null)).Returns(HostValue);
            this.provider.Setup(p => p.ReadIntegerOption("Port", 0)).Returns(PortValue);
            this.provider.Setup(p => p.ReadStringOption("Login", null)).Returns(LoginValue);
            this.provider.Setup(p => p.ReadStringOption("Password", null)).Returns(crypt.Encrypt(PasswordValue));
            this.provider.Setup(p => p.ReadStringOption("Domain", null)).Returns(DomainValue);


            var model = new NetworkSettingsModel(this.viewModel.Object, this.provider.Object);

            // Act
            model.Initialize();

            // Assert
            this.viewModel.VerifySet(vm => vm.ProxyMode = ProxyMode.Custom, Times.Once);
            this.viewModel.VerifySet(vm => vm.Host = HostValue, Times.Once);
            this.viewModel.VerifySet(vm => vm.Port = PortValue, Times.Once);
            this.viewModel.VerifySet(vm => vm.UserName = LoginValue, Times.Once);
            this.viewModel.VerifySet(vm => vm.Password = PasswordValue, Times.Once);
            this.viewModel.VerifySet(vm => vm.Domain = DomainValue, Times.Once);
        }
    }
}