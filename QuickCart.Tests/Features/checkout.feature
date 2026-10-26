@checkout
Feature: Checkout
  As a shopper
  I want to pay for my cart with my delivery details
  So that my order is confirmed

  Background:
    Given I am signed in as a shopper
    And I add "Smart Watch" to the cart 1 time
    And I add "USB-C Charger" to the cart 2 times
    And I open my cart
    And I go to checkout

  @smoke @e2e
  Scenario: Shopper places an order end to end
    When I place the order as "Sam Shopper" with email "sam@example.com" and postcode "BS1 4DJ"
    Then I see the order confirmation
    And the order total is "£168.98"
    And my cart shows 0 items

  @regression
  Scenario Outline: Invalid delivery details are rejected
    When I place the order as "<name>" with email "<email>" and postcode "<postcode>"
    Then I see the checkout error "<error>"

    Examples:
      | name        | email           | postcode | error                          |
      |             | sam@example.com | BS1 4DJ  | Name is required               |
      | Sam Shopper | not-an-email    | BS1 4DJ  | A valid email is required      |
      | Sam Shopper | sam@example.com | 12345    | A valid UK postcode is required |

  @regression @excel
  Scenario Outline: Checkout validation driven by Excel test data
    When I submit the checkout details from test data row "<row>"
    Then I see the checkout error from that test data row

    Examples:
      | row                  |
      | NAME_EMPTY           |
      | BAD_EMAIL            |
      | BAD_POSTCODE         |
      | BAD_POSTCODE_LETTERS |
