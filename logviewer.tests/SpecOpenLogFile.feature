Feature: SpecOpenLogFile
	Testing opening log file and parsing it

@mainmodel
Scenario: No filtering case
	Given I have file "test.log" on disk
	 And The file contains 2 messages with levels "INFO" and "ERROR"
	When I press open with default filtering parameters
	  And wait 2 seconds
	Then the number of shown messages should be 2

@mainmodel
Scenario: All messages filtered by level
	Given I have file "test.log" on disk
	  And The file contains 2 messages with levels "INFO" and "ERROR"
	When I press open with min level "Fatal" and max level "Fatal"
	  And wait 2 seconds
	Then the number of shown messages should be 0
	
@mainmodel
Scenario: Some messages filtered by level min equal max
	Given I have file "test.log" on disk
	  And The file contains 2 messages with levels "INFO" and "ERROR"
	When I press open with min level "Info" and max level "Info"
	  And wait 2 seconds
	Then the number of shown messages should be 1
	
@mainmodel
Scenario: Some messages filtered by level min less max
	Given I have file "test.log" on disk
	  And The file contains 2 messages with levels "INFO" and "ERROR"
	When I press open with min level "Trace" and max level "Info"
	  And wait 2 seconds
	Then the number of shown messages should be 1
	
@mainmodel
Scenario: Some messages filtered by level min greater max
	Given I have file "test.log" on disk
	  And The file contains 2 messages with levels "INFO" and "ERROR"
	When I press open with min level "Info" and max level "Warn"
	  And wait 2 seconds
	Then the number of shown messages should be 1

@mainmodel
Scenario: No filtering case. reload
	Given I have file "test.log" on disk
	  And The file contains 2 messages with levels "INFO" and "ERROR"
	When I press open with default filtering parameters
	  And wait 2 seconds
	  And I press reload
	  And wait 2 seconds
	Then the number of shown messages should be 2
	
@mainmodel
Scenario: Read last opened log on start
	Given I have file "test.log" on disk
	  And The file contains 2 messages with levels "INFO" and "ERROR"
	When I start application with default filtering parameters
	  And wait 2 seconds
	Then the number of shown messages should be 2

@mainmodel
Scenario: Some messages filtered by message text
	Given I have file "test.log" on disk
	  And The file contains 2 messages with levels "INFO" and "ERROR"
	When I press open with message text filter "message body 1"
	  And wait 2 seconds
	Then the number of shown messages should be 1
    
@mainmodel
Scenario: Some messages filtered by message text with regex support
	Given I have file "test.log" on disk
	  And The file contains 2 messages with levels "INFO" and "ERROR"
	When I press open with message text filter "message body \d+" and regular expression support enabled
	  And wait 2 seconds
	Then the number of shown messages should be 2
    
@mainmodel
Scenario: Some messages filtered by message text with regex support all messages filtered out
	Given I have file "test.log" on disk
	  And The file contains 2 messages with levels "INFO" and "ERROR"
	When I press open with message text filter "message body \d{3,}" and regular expression support enabled
	  And wait 2 seconds
	Then the number of shown messages should be 0