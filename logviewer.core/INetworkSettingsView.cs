// Created by: egr
// Created at: 05.09.2007
// © 2007-2008 Alexander Egorov

namespace logviewer.core
{
	public interface INetworkSettingsView
	{
		ProxyMode ProxyMode { get; set; }
		bool IsUseProxy { get; }
		bool IsUseIeProxy { get; }
		bool IsUseDefaultCredentials { get; set; }
		int Port { get; set; }
		string Host { get; set; }
		string UserName { get; set; }
		string Password { get; set; }
		string Domain { get; set; }
	}
}