using Microsoft.Playwright;

namespace QuickCart.Tests.Support;

/// <summary>Per-scenario browser context and page, shared between hooks and steps by Reqnroll context injection.</summary>
public sealed class BrowserSession
{
    public IBrowserContext Context { get; set; } = null!;

    public IPage Page { get; set; } = null!;
}
