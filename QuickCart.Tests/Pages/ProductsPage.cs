using Microsoft.Playwright;

namespace QuickCart.Tests.Pages;

public sealed class ProductsPage : BasePage
{
    public ProductsPage(IPage page)
        : base(page)
    {
    }

    public ILocator Search => Page.GetByTestId("search");

    public ILocator Cards => Page.GetByTestId("product-card");

    public ILocator NoResults => Page.GetByTestId("no-results");

    public ILocator Card(string name) => Page.Locator($"[data-testid='product-card'][data-product='{name}']");

    public ILocator Price(string name) => Card(name).GetByTestId("product-price");

    public ILocator AddButton(string name) => Card(name).GetByTestId("add-to-cart");

    public Task SearchForAsync(string term) => Search.FillAsync(term);

    public async Task AddAsync(string name, int times)
    {
        for (var i = 0; i < times; i++)
        {
            await AddButton(name).ClickAsync();
        }
    }
}
