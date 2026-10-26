@products
Feature: Browse and search products
  As a signed-in shopper
  I want to find products quickly
  So that I can add them to my cart

  Background:
    Given I am signed in as a shopper

  @smoke
  Scenario: All products are listed
    Then I see 6 products

  @regression
  Scenario Outline: Search narrows the product list
    When I search for "<term>"
    Then I see <count> products

    Examples:
      | term   | count |
      | watch  | 1     |
      | audio  | 0     |
      | band   | 1     |
      | SMART  | 1     |

  @regression
  Scenario: Searching for an unknown product shows a helpful message
    When I search for "television"
    Then I see the message "No products match your search"

  @regression
  Scenario: Sold-out products cannot be added
    Then the "Fitness Band" product cannot be added to the cart

  @smoke
  Scenario: Adding products updates the cart badge
    When I add "Wireless Headphones" to the cart 2 times
    And I add "USB-C Charger" to the cart 1 time
    Then my cart shows 3 items

  @regression @database
  Scenario: Prices shown in the shop match the reference database
    Then every product price on the page matches the reference database

  @regression @database
  Scenario: Out-of-stock products in the reference database are sold out in the shop
    Then every out-of-stock product in the reference database is sold out on the page
