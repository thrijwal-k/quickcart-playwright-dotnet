using Microsoft.Playwright;

namespace QuickCart.Tests.Pages;

public sealed class CheckoutPage
{
    private readonly IPage _page;

    public CheckoutPage(IPage page)
    {
        _page = page;
    }

    public ILocator Name => _page.GetByTestId("name");

    public ILocator Email => _page.GetByTestId("email");

    public ILocator Postcode => _page.GetByTestId("postcode");

    public ILocator PlaceOrderButton => _page.GetByTestId("place-order");

    public ILocator Errors => _page.GetByTestId("checkout-error");

    public ILocator Confirmation => _page.GetByTestId("confirmation");

    public ILocator OrderId => _page.GetByTestId("order-id");

    public ILocator OrderTotal => _page.GetByTestId("order-total");

    public async Task SubmitAsync(string name, string email, string postcode)
    {
        await Name.FillAsync(name);
        await Email.FillAsync(email);
        await Postcode.FillAsync(postcode);
        await PlaceOrderButton.ClickAsync();
    }
}
