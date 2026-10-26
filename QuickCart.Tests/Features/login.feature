@login
Feature: Sign in
  As a QuickCart customer
  I want to sign in securely
  So that I can shop and place orders

  Background:
    Given I am on the QuickCart sign-in page

  @smoke
  Scenario: Registered shopper signs in
    When I sign in as "shopper" with password "Passw0rd!"
    Then I see the welcome message "Hello, Sam Shopper"
    And my cart shows 0 items

  @regression
  Scenario Outline: Sign-in is refused with the right message
    When I sign in as "<username>" with password "<password>"
    Then I see the sign-in error "<message>"

    Examples:
      | username    | password  | message                           |
      | shopper     | wrong     | Username or password is incorrect |
      | nobody      | Passw0rd! | Username or password is incorrect |
      | locked_user | Passw0rd! | This account is locked            |
      |             |           | Username and password are required |

  @regression
  Scenario: Shopper signs out
    Given I sign in as "shopper" with password "Passw0rd!"
    When I sign out
    Then I am back on the sign-in page
