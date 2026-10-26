using Microsoft.Playwright;

namespace QuickCart.Tests.Pages;

public sealed class CartPage : BasePage
{
    public CartPage(IPage page)
        : base(page)
    {
    }

    public ILocator Lines => Page.GetByTestId("cart-line");

    public ILocator Subtotal => Page.GetByTestId("subtotal");

    public ILocator EmptyMessage => Page.GetByTestId("empty-cart");

    public ILocator Line(string name) => Page.Locator($"[data-testid='cart-line'][data-product='{name}']");

    public ILocator Quantity(string name) => Line(name).GetByTestId("line-qty");

    public Task IncreaseAsync(string name) =>
        Page.GetByRole(AriaRole.Button, new() { Name = $"Increase {name}" }).ClickAsync();

    public Task DecreaseAsync(string name) =>
        Page.GetByRole(AriaRole.Button, new() { Name = $"Decrease {name}" }).ClickAsync();

    public Task RemoveAsync(string name) => Line(name).GetByTestId("remove-line").ClickAsync();

    public Task CheckoutAsync() => Page.GetByTestId("checkout").ClickAsync();
}
