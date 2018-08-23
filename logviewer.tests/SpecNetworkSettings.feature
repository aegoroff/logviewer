Feature: SpecNetworkSettings
	Testing Network settings logic

@networksettings
Scenario: Change mode from None to Autodetection
	Given I have stored "None" option "ProxyMode"
	When I open network settings
		And Change mode to "AutoProxyDetection"
	Then I see IsUseAutoProxy set to "true"
		And IsNoUseProxy set to "false"
		And IsUseManualProxy set to "false"
		
@networksettings
Scenario: Change mode from None to Custom use default setting true
	Given I have stored "None" option "ProxyMode"
		And I have stored "localhost" option "Host"
		And I have stored 8080 option "Port"
		And I have stored "true" boolean option "IsUseDefaultCredentials"
	When I open network settings
		And Change mode to "Custom"
	Then IsUseManualProxy set to "true"
		And IsNoUseProxy set to "false"
		And I see IsUseAutoProxy set to "false"
		And Host set to "localhost"
		And Port set to 8080
		And IsUseDefaultCredentials set to "true"
		
@networksettings
Scenario: Change mode from None to Custom use default setting false
	Given I have stored "None" option "ProxyMode"
		And I have stored "localhost" option "Host"
		And I have stored 8080 option "Port"
		And I have stored "egr" option "Login"
		And I have crypted and stored "pwd" option "Password"
		And I have stored "egorov" option "Domain"
		And I have stored "false" boolean option "IsUseDefaultCredentials"
	When I open network settings
		And Change mode to "Custom"
	Then IsUseManualProxy set to "true"
		And IsNoUseProxy set to "false"
		And I see IsUseAutoProxy set to "false"
		And Host set to "localhost"
		And Port set to 8080
		And Login set to "egr"
		And Password set to "pwd"
		And Domain set to "egorov"
		And IsUseDefaultCredentials set to "false"

@networksettings
Scenario: Change mode from None to Custom use default setting true and then back to None
	Given I have stored "None" option "ProxyMode"
		And I have stored "localhost" option "Host"
		And I have stored 8080 option "Port"
		And I have stored "true" boolean option "IsUseDefaultCredentials"
	When I open network settings
		And Change mode to "Custom"
		And Change mode to "None"
	Then IsNoUseProxy set to "true"
		And IsUseManualProxy set to "false"
		And I see IsUseAutoProxy set to "false"
		And Host set to null
		And Port set to 0
		And IsUseDefaultCredentials set to "true"

@networksettings
Scenario: Change mode from None to Custom use default setting false and then back to None
	Given I have stored "None" option "ProxyMode"
		And I have stored "localhost" option "Host"
		And I have stored 8080 option "Port"
		And I have stored "egr" option "Login"
		And I have crypted and stored "pwd" option "Password"
		And I have stored "egorov" option "Domain"
		And I have stored "false" boolean option "IsUseDefaultCredentials"
	When I open network settings
		And Change mode to "Custom"
		And Change mode to "None"
	Then IsNoUseProxy set to "true"
		And IsUseManualProxy set to "false"
		And I see IsUseAutoProxy set to "false"
		And Host set to null
		And Port set to 0
		And IsUseDefaultCredentials set to "false"