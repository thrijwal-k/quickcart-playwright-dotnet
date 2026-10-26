@cart
Feature: Manage the cart
  As a shopper
  I want to change quantities and remove items
  So that I only pay for what I want

  Background:
    Given I am signed in as a shopper
    And I add "Bluetooth Speaker" to the cart 2 times
    And I add "Laptop Stand" to the cart 1 time
    And I open my cart

  @smoke
  Scenario: Cart shows the right lines and subtotal
    Then my cart has 2 lines
    And the subtotal is "£96.75"

  @regression
  Scenario: Increasing and decreasing quantities updates the subtotal
    When I increase the quantity of "Laptop Stand"
    And I decrease the quantity of "Bluetooth Speaker"
    Then the quantity of "Laptop Stand" is 2
    And the quantity of "Bluetooth Speaker" is 1
    And the subtotal is "£90.00"

  @regression
  Scenario: Removing every line empties the cart
    When I remove "Bluetooth Speaker" from the cart
    And I remove "Laptop Stand" from the cart
    Then I see the message "Your cart is empty"
    And my cart shows 0 items
