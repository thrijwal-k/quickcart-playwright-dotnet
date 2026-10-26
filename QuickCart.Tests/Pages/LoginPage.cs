using Microsoft.Playwright;

namespace QuickCart.Tests.Pages;

public sealed class LoginPage
{
    private readonly IPage _page;

    public LoginPage(IPage page)
    {
        _page = page;
    }

    public ILocator Username => _page.GetByTestId("username");

    public ILocator Password => _page.GetByTestId("password");

    public ILocator SubmitButton => _page.GetByTestId("login-button");

    public ILocator Error => _page.GetByTestId("login-error");

    public async Task OpenAsync()
    {
        await _page.GotoAsync("/");
    }

    public async Task SignInAsync(string username, string password)
    {
        await Username.FillAsync(username);
        await Password.FillAsync(password);
        await SubmitButton.ClickAsync();
    }
}
