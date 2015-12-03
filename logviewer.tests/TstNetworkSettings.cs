// Created by: egr
// Created at: 04.09.2007
// © 2007-2015 Alexander Egorov

using FluentAssertions;
using logviewer.core;
using Moq;
using Xunit;

namespace logviewer.tests
{
    public class TstNetworkSettings
    {
        private readonly Mock<IOptionsProvider> provider;
        private readonly NetworkSettings settings;
        private readonly AsymCrypt crypt;


        public TstNetworkSettings()
        {
            this.provider = new Mock<IOptionsProvider>();
            this.crypt = new AsymCrypt();
            this.crypt.GenerateKeys();
            this.provider.Setup(p => p.ReadStringOption("PrivateKey", null)).Returns(this.crypt.PrivateKey);
            this.provider.Setup(p => p.ReadStringOption("PublicKey", null)).Returns(this.crypt.PublicKey);
            this.settings = new NetworkSettings(this.provider.Object);
        }

        [Fact]
        public void Host_GetSet_GetShouldBeTheSameAsSet()
        {
            // Arrange
            const string host = "h";

            this.provider.Setup(p => p.UpdateStringOption("Host", host));
            this.provider.Setup(p => p.ReadStringOption("Host", null)).Returns(host);

            // Act
            this.settings.Host = host;

            // Assert
            this.settings.Host.Should().Be(host);
        }

        [Fact]
        public void Login_GetSet_GetShouldBeTheSameAsSet()
        {
            // Arrange
            const string userName = "l";

            this.provider.Setup(p => p.UpdateStringOption("Login", userName));
            this.provider.Setup(p => p.ReadStringOption("Login", null)).Returns(userName);

            // Act
            this.settings.UserName = userName;

            // Assert
            this.settings.UserName.Should().Be(userName);
        }

        [Fact]
        public void Password_GetSet_GetShouldBeTheSameAsSet()
        {
            // Arrange
            const string password = "p";

            this.provider.Setup(p => p.UpdateStringOption("Password", It.IsAny<string>()));
            this.provider.Setup(p => p.ReadStringOption("Password", null)).Returns(this.crypt.Encrypt(password));

            // Act
            this.settings.Password = password;

            // Assert
            this.settings.Password.Should().Be(password);
        }

        [Fact]
        public void Domain_GetSet_GetShouldBeTheSameAsSet()
        {
            // Arrange
            const string domain = "d";

            this.provider.Setup(p => p.UpdateStringOption("Domain", domain));
            this.provider.Setup(p => p.ReadStringOption("Domain", null)).Returns(domain);

            // Act
            this.settings.Domain = domain;

            // Assert
            this.settings.Domain.Should().Be(domain);
        }

        [Fact]
        public void IsUseProxy_GetSet_GetShouldBeTheSameAsSet()
        {
            // Arrange
            this.provider.Setup(p => p.UpdateBooleanOption("IsUseProxy", true));
            this.provider.Setup(p => p.ReadBooleanOption("IsUseProxy", false)).Returns(true);

            // Act
            this.settings.IsUseProxy = true;

            // Assert
            this.settings.IsUseProxy.Should().Be(true);
        }

        [Fact]
        public void IsUseIeProxy_GetSet_GetShouldBeTheSameAsSet()
        {
            // Arrange
            this.provider.Setup(p => p.UpdateBooleanOption("IsUseIeProxy", false));
            this.provider.Setup(p => p.ReadBooleanOption("IsUseIeProxy", true)).Returns(false);

            // Act
            this.settings.IsUseIeProxy = false;

            // Assert
            this.settings.IsUseIeProxy.Should().Be(false);
        }

        [Fact]
        public void IsDefaultCredentials_GetSet_GetShouldBeTheSameAsSet()
        {
            // Arrange
            this.provider.Setup(p => p.UpdateBooleanOption("IsUseDefaultCredentials", false));
            this.provider.Setup(p => p.ReadBooleanOption("IsUseDefaultCredentials", true)).Returns(false);

            // Act
            this.settings.IsUseDefaultCredentials = false;

            // Assert
            this.settings.IsUseDefaultCredentials.Should().Be(false);
        }

        [Fact]
        public void Port()
        {
            // Arrange
            const int port = 777;

            this.provider.Setup(p => p.UpdateIntegerOption("Port", port));
            this.provider.Setup(p => p.ReadIntegerOption("Port", 0)).Returns(port);

            // Act
            this.settings.Port = port;

            // Assert
            this.settings.Port.Should().Be(port);
        }
    }
}
