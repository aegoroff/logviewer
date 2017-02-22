Feature: SpecOpenLogFile
	Testing opening log file and parsing it

@mainmodel
Scenario: No filtering case
	Given I have file "test.log" on disk
	When I press open with default filtering parameters
	  And wait 1 seconds
	Then the number of shown messages should be 2

@mainmodel
Scenario: Filtering by level
	Given I have file "test.log" on disk
	When I press open with min level "Fatal" and max level "Fatal"
	  And wait 1 seconds
	Then the number of shown messages should be 0

@mainmodel
Scenario: No filtering case. reload
	Given I have file "test.log" on disk
	When I press open with default filtering parameters
	  And wait 1 seconds
	  And I press reload
	  And wait 1 seconds
	Then the number of shown messages should be 2