using System.Globalization;
using Microsoft.Playwright;
using QuickCart.Tests.Pages;
using QuickCart.Tests.Support;
using Reqnroll;
using static Microsoft.Playwright.Assertions;

namespace QuickCart.Tests.StepDefinitions;

/// <summary>
/// Step definitions stay thin and business-readable: every locator lives in a page object,
/// and every assertion uses Playwright's auto-retrying Expect (no hard sleeps).
/// </summary>
[Binding]
public sealed class ShopSteps
{
    private const string ShopperUsername = "shopper";
    private const string ShopperPassword = "Passw0rd!";

    private readonly BrowserSession _session;
    private readonly ScenarioContext _scenario;

    public ShopSteps(BrowserSession session, ScenarioContext scenario)
    {
        _session = session;
        _scenario = scenario;
    }

    private IPage Page => _session.Page;

    private LoginPage Login => new(Page);

    private ProductsPage Products => new(Page);

    private CartPage Cart => new(Page);

    private CheckoutPage Checkout => new(Page);

    // ---------- Sign in ----------
    [Given("I am on the QuickCart sign-in page")]
    public Task GivenIAmOnTheSignInPage() => Login.OpenAsync();

    [Given("I am signed in as a shopper")]
    public async Task GivenIAmSignedInAsAShopper()
    {
        await Login.OpenAsync();
        await Login.SignInAsync(ShopperUsername, ShopperPassword);
        await Expect(Products.Welcome).ToBeVisibleAsync();
    }

    [Given("I sign in as {string} with password {string}")]
    [When("I sign in as {string} with password {string}")]
    public Task WhenISignIn(string username, string password) => Login.SignInAsync(username, password);

    [When("I sign out")]
    public Task WhenISignOut() => Products.SignOutAsync();

    [Then("I see the welcome message {string}")]
    public Task ThenISeeTheWelcomeMessage(string message) => Expect(Products.Welcome).ToHaveTextAsync(message);

    [Then("I see the sign-in error {string}")]
    public Task ThenISeeTheSignInError(string message) => Expect(Login.Error).ToHaveTextAsync(message);

    [Then("I am back on the sign-in page")]
    public Task ThenIAmBackOnTheSignInPage() => Expect(Login.SubmitButton).ToBeVisibleAsync();

    // ---------- Products ----------
    [When("I search for {string}")]
    public Task WhenISearchFor(string term) => Products.SearchForAsync(term);

    [Then("I see {int} products")]
    public Task ThenISeeProducts(int count) => Expect(Products.Cards).ToHaveCountAsync(count);

    [Then("the {string} product cannot be added to the cart")]
    public async Task ThenTheProductCannotBeAdded(string name)
    {
        await Expect(Products.AddButton(name)).ToBeDisabledAsync();
        await Expect(Products.AddButton(name)).ToHaveTextAsync("Sold out");
    }

    [Given(@"^I add ""(.*)"" to the cart (\d+) times?$")]
    [When(@"^I add ""(.*)"" to the cart (\d+) times?$")]
    public Task WhenIAddToTheCart(string name, int times) => Products.AddAsync(name, times);

    [Then("every product price on the page matches the reference database")]
    public async Task ThenEveryPriceMatchesTheReferenceDatabase()
    {
        var expected = ReferenceDatabase.QueryProducts();
        await Expect(Products.Cards).ToHaveCountAsync(expected.Count);
        foreach (var product in expected)
        {
            var price = "£" + product.Price.ToString("0.00", CultureInfo.InvariantCulture);
            await Expect(Products.Price(product.Name)).ToHaveTextAsync(price);
        }
    }

    [Then("every out-of-stock product in the reference database is sold out on the page")]
    public async Task ThenOutOfStockProductsAreSoldOut()
    {
        var soldOut = ReferenceDatabase.QueryProducts("stock = 0");
        foreach (var product in soldOut)
        {
            await Expect(Products.AddButton(product.Name)).ToBeDisabledAsync();
        }
    }

    // ---------- Cart ----------
    [Given("I open my cart")]
    [When("I open my cart")]
    public Task WhenIOpenMyCart() => Products.GoToCartAsync();

    [Then(@"^my cart shows (\d+) items?$")]
    public Task ThenMyCartShowsItems(int count) =>
        Expect(Products.CartCount).ToHaveTextAsync(count.ToString(CultureInfo.InvariantCulture));

    [Then("my cart has {int} lines")]
    public Task ThenMyCartHasLines(int count) => Expect(Cart.Lines).ToHaveCountAsync(count);

    [Then("the subtotal is {string}")]
    public Task ThenTheSubtotalIs(string amount) => Expect(Cart.Subtotal).ToHaveTextAsync(amount);

    [When("I increase the quantity of {string}")]
    public Task WhenIIncrease(string name) => Cart.IncreaseAsync(name);

    [When("I decrease the quantity of {string}")]
    public Task WhenIDecrease(string name) => Cart.DecreaseAsync(name);

    [When("I remove {string} from the cart")]
    public Task WhenIRemove(string name) => Cart.RemoveAsync(name);

    [Then("the quantity of {string} is {int}")]
    public Task ThenTheQuantityIs(string name, int quantity) =>
        Expect(Cart.Quantity(name)).ToHaveTextAsync(quantity.ToString(CultureInfo.InvariantCulture));

    [Then("I see the message {string}")]
    public Task ThenISeeTheMessage(string message) =>
        Expect(Page.GetByText(message, new() { Exact = true })).ToBeVisibleAsync();

    // ---------- Checkout ----------
    [Given("I go to checkout")]
    public Task GivenIGoToCheckout() => Cart.CheckoutAsync();

    [When("I place the order as {string} with email {string} and postcode {string}")]
    public Task WhenIPlaceTheOrder(string name, string email, string postcode) =>
        Checkout.SubmitAsync(name, email, postcode);

    [When("I submit the checkout details from test data row {string}")]
    public async Task WhenISubmitCheckoutDetailsFromTestData(string rowId)
    {
        var testCase = ExcelTestData.GetCheckoutCase(rowId);
        _scenario.Set(testCase);
        TestLog.Logger.Information("Using Excel test data row {Row}", testCase.Id);
        await Checkout.SubmitAsync(testCase.Name, testCase.Email, testCase.Postcode);
    }

    [Then("I see the checkout error from that test data row")]
    public Task ThenISeeTheExpectedErrorFromTestData()
    {
        var testCase = _scenario.Get<CheckoutCase>();
        return Expect(Checkout.Errors).ToContainTextAsync(new[] { testCase.ExpectedError });
    }

    [Then("I see the order confirmation")]
    public async Task ThenISeeTheOrderConfirmation()
    {
        await Expect(Checkout.Confirmation).ToHaveTextAsync("Order confirmed");
        await Expect(Checkout.OrderId).ToHaveTextAsync(new System.Text.RegularExpressions.Regex(@"^\d+$"));
    }

    [Then("the order total is {string}")]
    public Task ThenTheOrderTotalIs(string amount) => Expect(Checkout.OrderTotal).ToHaveTextAsync(amount);

    [Then("I see the checkout error {string}")]
    public Task ThenISeeTheCheckoutError(string error) =>
        Expect(Checkout.Errors).ToContainTextAsync(new[] { error });
}
