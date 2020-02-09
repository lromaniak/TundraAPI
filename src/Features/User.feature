@tundra.com
Feature: Verify user data
	In order to manage user data and posts
	As a user
	I want to be able to use exposed APIs

Background: 
Given Generate random number between 1 and 10 as userID

Scenario: User has correct email address
	When Get user details
	Then Response status is 200
    And Email format is valid

Scenario: User posts has correct content and user add new post succesfully
	When Get user associated posts
	Then Response status is 200
	And Posts has correct structure
	When User do a new post with title and body:
	| title         | body                      |
	| my test title | tundra body random string |
	Then Response status is 201