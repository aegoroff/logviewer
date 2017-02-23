// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 16.12.2015
// © 2012-2017 Alexander Egorov

using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using FluentAssertions;
using logviewer.logic.ui.network;
using Xunit;

namespace logviewer.tests
{
    public class TstNetworkSettingsViewModel
    {
        private const string TestString = "str";
        private const int TestInt = 10;

        [Fact]
        public void ProxyMode_Set_OnPropertyChangedInvokedForThreeProperties()
        {
            // Arrange
            var calls = new List<string>();
            var model = new NetworkSettingsViewModel();
            model.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { calls.Add(args.PropertyName); };

            // Act
            model.ProxyMode = ProxyMode.Custom;

            // Assert
            calls.Should().BeEquivalentTo(nameof(model.IsNoUseProxy), nameof(model.IsUseAutoProxy), nameof(model.IsUseManualProxy));
            model.ProxyMode.Should().Be(ProxyMode.Custom);
        }

        [Fact]
        public void ProxyMode_SetNone_IsNoUseProxyTrueOtherFalse()
        {
            // Arrange
            var model = new NetworkSettingsViewModel();

            // Act
            model.ProxyMode = ProxyMode.None;

            // Assert
            model.IsNoUseProxy.Should().BeTrue();
            model.IsUseAutoProxy.Should().BeFalse();
            model.IsUseManualProxy.Should().BeFalse();
        }

        [Fact]
        public void ProxyMode_SetAutoProxyDetection_IsUseAutoProxyTrueOtherFalse()
        {
            // Arrange
            var model = new NetworkSettingsViewModel();

            // Act
            model.ProxyMode = ProxyMode.AutoProxyDetection;

            // Assert
            model.IsNoUseProxy.Should().BeFalse();
            model.IsUseAutoProxy.Should().BeTrue();
            model.IsUseManualProxy.Should().BeFalse();
        }

        [Fact]
        public void ProxyMode_SetCustom_IsUseManualProxyTrueOtherFalse()
        {
            // Arrange
            var model = new NetworkSettingsViewModel();

            // Act
            model.ProxyMode = ProxyMode.Custom;

            // Assert
            model.IsNoUseProxy.Should().BeFalse();
            model.IsUseAutoProxy.Should().BeFalse();
            model.IsUseManualProxy.Should().BeTrue();
        }

        [Fact]
        public void IsNoUseProxy_Set_ModeChangedWithNone()
        {
            // Arrange
            var proxyMode = (ProxyMode) 10000;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate(object sender, ProxyMode mode) { proxyMode = mode; };

            // Act
            model.IsNoUseProxy = true;

            // Assert
            proxyMode.Should().Be(ProxyMode.None);
            model.IsNoUseProxy.Should().BeTrue();
        }

        [Fact]
        public void IsUseAutoProxy_Set_ModeChangedWithAutoProxyDetection()
        {
            // Arrange
            var proxyMode = (ProxyMode) 10000;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate(object sender, ProxyMode mode) { proxyMode = mode; };

            // Act
            model.IsUseAutoProxy = true;

            // Assert
            proxyMode.Should().Be(ProxyMode.AutoProxyDetection);
            model.IsUseAutoProxy.Should().BeTrue();
        }

        [Fact]
        public void IsUseManualProxy_Set_ModeChangedWithCustom()
        {
            // Arrange
            var proxyMode = (ProxyMode) 10000;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate(object sender, ProxyMode mode) { proxyMode = mode; };

            // Act
            model.IsUseManualProxy = true;

            // Assert
            proxyMode.Should().Be(ProxyMode.Custom);
            model.IsUseManualProxy.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsUseDefaultCredentials_Set_ModeChangedWithCustomPropertyChanged(bool value)
        {
            // Arrange
            var proxyMode = (ProxyMode) 10000;
            string property = null;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate(object sender, ProxyMode mode) { proxyMode = mode; };
            model.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { property = args.PropertyName; };

            // Act
            model.IsUseDefaultCredentials = value;

            // Assert
            proxyMode.Should().Be(ProxyMode.Custom);
            property.Should().Be(nameof(model.EnableCustomCredentials));
            model.IsUseDefaultCredentials.Should().Be(value);
        }

        [Fact]
        public void IsNoUseProxy_SetTwiceToTrue_ModeChangedCalledOnce()
        {
            // Arrange
            var count = 0;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate { count++; };

            // Act
            model.IsNoUseProxy = true;
            model.IsNoUseProxy = true;

            // Assert
            count.Should().Be(1);
        }

        [Fact]
        public void IsNoUseProxy_SetTrueTrueFalse_ModeChangedCalledOnce()
        {
            // Arrange
            var count = 0;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate { count++; };

            // Act
            model.IsNoUseProxy = true;
            model.IsNoUseProxy = true;
            model.IsNoUseProxy = false;

            // Assert
            count.Should().Be(1);
        }

        [Fact]
        public void IsNoUseProxy_SetTrueFalseTrue_ModeChangedCalledTwice()
        {
            // Arrange
            var count = 0;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate { count++; };

            // Act
            model.IsNoUseProxy = true;
            model.IsNoUseProxy = false;
            model.IsNoUseProxy = true;

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public void IsUseAutoProxy_SetTwiceToTrue_ModeChangedCalledOnce()
        {
            // Arrange
            var count = 0;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate { count++; };

            // Act
            model.IsUseAutoProxy = true;
            model.IsUseAutoProxy = true;

            // Assert
            count.Should().Be(1);
        }

        [Fact]
        public void IsUseAutoProxy_SetTrueTrueFalse_ModeChangedCalledOnce()
        {
            // Arrange
            var count = 0;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate { count++; };

            // Act
            model.IsUseAutoProxy = true;
            model.IsUseAutoProxy = true;
            model.IsUseAutoProxy = false;

            // Assert
            count.Should().Be(1);
        }

        [Fact]
        public void IsUseAutoProxy_SetTrueFalseTrue_ModeChangedCalledTwice()
        {
            // Arrange
            var count = 0;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate { count++; };

            // Act
            model.IsUseAutoProxy = true;
            model.IsUseAutoProxy = false;
            model.IsUseAutoProxy = true;

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public void IsUseManualProxy_SetTwiceToTrue_ModeChangedCalledOnce()
        {
            // Arrange
            var count = 0;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate { count++; };

            // Act
            model.IsUseManualProxy = true;
            model.IsUseManualProxy = true;

            // Assert
            count.Should().Be(1);
        }

        [Fact]
        public void IsUseManualProxy_SetTrueTrueFalse_ModeChangedCalledOnce()
        {
            // Arrange
            var count = 0;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate { count++; };

            // Act
            model.IsUseManualProxy = true;
            model.IsUseManualProxy = true;
            model.IsUseManualProxy = false;

            // Assert
            count.Should().Be(1);
        }

        [Fact]
        public void IsUseManualProxy_SetTrueFalseTrue_ModeChangedCalledTwice()
        {
            // Arrange
            var count = 0;
            var model = new NetworkSettingsViewModel();
            model.ModeChanged += delegate { count++; };

            // Act
            model.IsUseManualProxy = true;
            model.IsUseManualProxy = false;
            model.IsUseManualProxy = true;

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public void Host_Set_PropertyChangedWithPropertyName()
        {
            // Arrange
            var calls = new List<string>();
            var model = new NetworkSettingsViewModel();
            model.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { calls.Add(args.PropertyName); };

            // Act
            model.Host = TestString;

            // Assert
            calls.Should().BeEquivalentTo(nameof(model.Host));
            model.Host.Should().Be(TestString);
        }

        [Fact]
        public void Port_Set_PropertyChangedWithPropertyName()
        {
            // Arrange
            var calls = new List<string>();
            var model = new NetworkSettingsViewModel();
            model.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { calls.Add(args.PropertyName); };

            // Act
            model.Port = TestInt;

            // Assert
            calls.Should().BeEquivalentTo(nameof(model.Port));
            model.Port.Should().Be(TestInt);
        }

        [Fact]
        public void UserName_Set_PropertyChangedWithPropertyName()
        {
            // Arrange
            var calls = new List<string>();
            var model = new NetworkSettingsViewModel();
            model.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { calls.Add(args.PropertyName); };

            // Act
            model.UserName = TestString;

            // Assert
            calls.Should().BeEquivalentTo(nameof(model.UserName));
            model.UserName.Should().Be(TestString);
        }

        [Fact]
        public void Password_Set_PropertyChangedWithPropertyName()
        {
            // Arrange
            var calls = new List<string>();
            var model = new NetworkSettingsViewModel();
            model.PasswordUpdated += delegate(object sender, string s) { calls.Add(s); };

            // Act
            model.Password = TestString;

            // Assert
            calls.Should().BeEquivalentTo(TestString);
            model.Password.Should().Be(TestString);
        }

        [Fact]
        public void Domain_Set_PropertyChangedWithPropertyName()
        {
            // Arrange
            var calls = new List<string>();
            var model = new NetworkSettingsViewModel();
            model.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { calls.Add(args.PropertyName); };

            // Act
            model.Domain = TestString;

            // Assert
            calls.Should().BeEquivalentTo(nameof(model.Domain));
            model.Domain.Should().Be(TestString);
        }

        [Fact]
        public void IsSettingsChanged_Set_PropertyChangedWithPropertyName()
        {
            // Arrange
            var calls = new List<string>();
            var model = new NetworkSettingsViewModel();
            model.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { calls.Add(args.PropertyName); };

            // Act
            model.IsSettingsChanged = true;

            // Assert
            calls.Should().BeEquivalentTo(nameof(model.IsSettingsChanged));
            model.IsSettingsChanged.Should().BeTrue();
        }

        [Fact]
        public void Proxy_ModeNone_ReturnNull()
        {
            // Arrange
            var model = new NetworkSettingsViewModel { ProxyMode = ProxyMode.None };

            // Act
            var proxy = model.Proxy;

            // Assert
            proxy.Should().BeNull();
        }

        [Fact]
        public void Proxy_ModeInvalid_ReturnNull()
        {
            // Arrange
            var model = new NetworkSettingsViewModel { ProxyMode = (ProxyMode)1000 };

            // Act
            var proxy = model.Proxy;

            // Assert
            proxy.Should().BeNull();
        }

        [Fact]
        public void Proxy_ModeAuto_ReturnSystemWithDefaultCredentials()
        {
            // Arrange
            var model = new NetworkSettingsViewModel { ProxyMode = ProxyMode.AutoProxyDetection };

            // Act
            var proxy = model.Proxy;

            // Assert
            proxy.Should().NotBeNull();
            proxy.Credentials.Should().NotBeNull();
        }

        [Fact]
        public void Proxy_ModeCustomIsUseDefaultCredentials_ReturnCustomWithDefaultCredentials()
        {
            // Arrange
            const string host = "localhost";
            const int port = 8080;
            var model = new NetworkSettingsViewModel
            {
                ProxyMode = ProxyMode.Custom,
                Host = host,
                Port = port,
                IsUseDefaultCredentials = true
            };

            // Act
            var proxy = (WebProxy) model.Proxy;

            // Assert
            proxy.Address.Host.Should().Be(host);
            proxy.Address.Port.Should().Be(port);
            proxy.Credentials.Should().NotBeNull();
        }

        [Fact]
        public void Proxy_ModeCustomNotIsUseDefaultCredentials_ReturnCustomWithCredentials()
        {
            // Arrange
            const string host = "localhost";
            const string login = "login";
            const string password = "123";
            const string domain = "domain";
            const int port = 8080;
            var model = new NetworkSettingsViewModel
            {
                ProxyMode = ProxyMode.Custom,
                Host = host,
                Port = port,
                UserName = login,
                Password = password,
                Domain = domain,
                IsUseDefaultCredentials = false
            };

            // Act
            var proxy = (WebProxy) model.Proxy;
            var credentials = (NetworkCredential) proxy.Credentials;

            // Assert
            proxy.Address.Host.Should().Be(host);
            proxy.Address.Port.Should().Be(port);
            credentials.UserName.Should().Be(login);
            credentials.Password.Should().Be(password);
            credentials.Domain.Should().Be(domain);
        }
    }
}