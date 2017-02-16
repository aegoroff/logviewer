Feature: SpecReadLog
	Reading log tests

@readlog
Scenario: Read log from memory stream
	Given I have grok "^\[?%{TIMESTAMP_ISO8601:Occured:DateTime}\]?%{DATA}%{LOGLEVEL:Level:LogLevel}%{DATA}"
	And I have "2008-12-27 19:31:47,250 [4688] INFO" message in the stream
	When I start read log from stream
	Then the read result should be "2008-12-27 19:31:47,250 [4688] INFO"