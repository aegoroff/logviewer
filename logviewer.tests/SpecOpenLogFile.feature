Feature: SpecOpenLogFile
	Testing opening log file and parsing it

@mainmodel
Scenario: No filtering case
	Given I have file "test.log" on disk
	When I press open with default filtering parameters
	Then the number of shown messages should be 2

@mainmodel
Scenario: Filtering by level
	Given I have file "test.log" on disk
	When I press open with min level "Fatal" and max level "Fatal"
	Then the number of shown messages should be 0

@mainmodel
Scenario: No filtering case. reload
	Given I have file "test.log" on disk
	When I press open with default filtering parameters and then reload
	Then the number of shown messages should be 2