// Created by: egr
// Created at: 05.09.2007
// © 2007-2008 Alexander Egorov

using logviewer.logic;
using logviewer.logic.ui.network;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstNetworkSettingsModel
    {
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
    }
}