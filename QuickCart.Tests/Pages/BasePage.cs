using Microsoft.Playwright;

namespace QuickCart.Tests.Pages;

/// <summary>Header elements and navigation shared by every signed-in page.</summary>
public abstract class BasePage
{
    protected BasePage(IPage page)
    {
        Page = page;
    }

    protected IPage Page { get; }

    public ILocator CartCount => Page.GetByTestId("cart-count");

    public ILocator Welcome => Page.GetByTestId("welcome");

    public Task GoToCartAsync() => Page.GetByTestId("nav-cart").ClickAsync();

    public Task GoToProductsAsync() => Page.GetByTestId("nav-products").ClickAsync();

    public Task SignOutAsync() => Page.GetByTestId("logout").ClickAsync();
}
