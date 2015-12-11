// Created by: egr
// Created at: 05.09.2007
// © 2007-2008 Alexander Egorov

using logviewer.logic;
using logviewer.logic.models;
using logviewer.logic.ui;
using logviewer.logic.ui.network;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstNetworkSettingsController
    {
        private readonly Mock<IOptionsProvider> provider;
        private readonly Mock<INetworkSettingsView> view;

        public TstNetworkSettingsController()
        {
            this.provider = new Mock<IOptionsProvider>();
            this.view = new Mock<INetworkSettingsView>();
        }

        [Fact]
        public void Write_CustomProxyWithDefaultCredentials_CustomCredentialsNotAffected()
        {
            // Arrange
            this.view.SetupGet(v => v.ProxyMode).Returns(ProxyMode.Custom);
            this.view.SetupGet(v => v.Host).Returns("host");
            this.view.SetupGet(v => v.Port).Returns(1234);
            this.view.SetupGet(v => v.IsUseProxy).Returns(true);
            this.view.SetupGet(v => v.IsUseIeProxy).Returns(false);
            this.view.SetupGet(v => v.IsUseDefaultCredentials).Returns(true);
            this.view.SetupGet(v => v.UserName).Returns("l2");
            this.view.SetupGet(v => v.Password).Returns("p2");
            this.view.SetupGet(v => v.Domain).Returns("d2");

            var settings = new NetworkSettings(this.provider.Object);
            var controller = new NetworkSettingsController(settings, this.view.Object);

            // Act
            controller.Write();

            // Assert
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseProxy", true), Times.Once);
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseIeProxy", false), Times.Once);
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
            this.view.SetupGet(v => v.ProxyMode).Returns(ProxyMode.Custom);
            this.view.SetupGet(v => v.Host).Returns(host);
            this.view.SetupGet(v => v.Port).Returns(port);
            this.view.SetupGet(v => v.IsUseProxy).Returns(true);
            this.view.SetupGet(v => v.IsUseIeProxy).Returns(false);
            this.view.SetupGet(v => v.IsUseDefaultCredentials).Returns(true);
            this.view.SetupGet(v => v.UserName).Returns("l2");
            this.view.SetupGet(v => v.Password).Returns("p2");
            this.view.SetupGet(v => v.Domain).Returns("d2");

            var settings = new NetworkSettings(this.provider.Object);
            var controller = new NetworkSettingsController(settings, this.view.Object);

            // Act
            controller.Write();

            // Assert
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseProxy", true), Times.Once);
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseIeProxy", false), Times.Once);
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
            this.view.SetupGet(v => v.ProxyMode).Returns(ProxyMode.None);
            this.view.SetupGet(v => v.Host).Returns("host1");
            this.view.SetupGet(v => v.Port).Returns(12341);
            this.view.SetupGet(v => v.IsUseDefaultCredentials).Returns(false);
            this.view.SetupGet(v => v.IsUseProxy).Returns(false);
            this.view.SetupGet(v => v.IsUseIeProxy).Returns(false);

            var settings = new NetworkSettings(this.provider.Object);
            var controller = new NetworkSettingsController(settings, this.view.Object);

            // Act
            controller.Write();

            // Assert
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseProxy", false), Times.Once);
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseIeProxy", false), Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Host", It.IsAny<string>()), Times.Never);
            this.provider.Verify(p => p.UpdateIntegerOption("Port", It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Write_CustomModeWithCredentials_AllSet()
        {
            this.view.SetupGet(v => v.ProxyMode).Returns(ProxyMode.Custom);
            this.view.SetupGet(v => v.Host).Returns("host2");
            this.view.SetupGet(v => v.Port).Returns(123411);
            this.view.SetupGet(v => v.IsUseProxy).Returns(true);
            this.view.SetupGet(v => v.IsUseIeProxy).Returns(false);
            this.view.SetupGet(v => v.IsUseDefaultCredentials).Returns(false);
            this.view.SetupGet(v => v.UserName).Returns("l1");
            this.view.SetupGet(v => v.Password).Returns("p1");
            this.view.SetupGet(v => v.Domain).Returns("d1");

            var settings = new NetworkSettings(this.provider.Object);
            var controller = new NetworkSettingsController(settings, this.view.Object);

            // Act
            controller.Write();

            // Assert
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseProxy", true), Times.Once);
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseIeProxy", false), Times.Once);
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseDefaultCredentials", false), Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Host", "host2"), Times.Once);
            this.provider.Verify(p => p.UpdateIntegerOption("Port", 123411), Times.Once);

            this.provider.Verify(p => p.UpdateStringOption("Login", "l1"), Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Password", "p1"), Times.Never); // Don't save plain passwords
            this.provider.Verify(p => p.UpdateStringOption("Password", It.IsAny<string>()), Times.Once);
            this.provider.Verify(p => p.UpdateStringOption("Domain", "d1"), Times.Once);
        }

        [Fact]
        public void Write_AutoProxy_OnlyProxyFlagsAffected()
        {
            // Arrange
            this.view.SetupGet(v => v.ProxyMode).Returns(ProxyMode.AutoProxyDetection);
            this.view.SetupGet(v => v.IsUseProxy).Returns(false);
            this.view.SetupGet(v => v.IsUseIeProxy).Returns(true);

            var settings = new NetworkSettings(this.provider.Object);
            var controller = new NetworkSettingsController(settings, this.view.Object);
            controller.Write();

            // Assert
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseProxy", false), Times.Once);
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseIeProxy", true), Times.Once);

            this.provider.Verify(p => p.UpdateStringOption("Host", It.IsAny<string>()), Times.Never);
            this.provider.Verify(p => p.UpdateIntegerOption("Port", It.IsAny<int>()), Times.Never);
            this.provider.Verify(p => p.UpdateBooleanOption("IsUseDefaultCredentials", It.IsAny<bool>()), Times.Never);
        }
    }
}