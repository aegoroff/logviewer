// Created by: egr
// Created at: 13.12.2015
// © 2012-2016 Alexander Egorov

using System;

namespace logviewer.logic.ui.network
{
    public interface INetworkSettingsViewModel
    {
        void Initialize(ProxyMode mode, bool useDefaultCredentials);
        ProxyMode ProxyMode { get; set; }
        bool IsNoUseProxy { get; set; }
        bool IsUseAutoProxy { get; set; }
        bool IsUseManualProxy { get; set; }
        bool IsUseDefaultCredentials { get; set; }
        bool IsSettingsChanged { get; set; }
        string Host { get; set; }
        int Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string Domain { get; set; }
        event EventHandler<ProxyMode> ModeChanged;
        event EventHandler<string> PasswordUpdated;
    }
}