using Microsoft.Playwright;
using NUnit.Framework;
using QuickCart.Tests.Support;
using Reqnroll;

namespace QuickCart.Tests.Hooks;

/// <summary>
/// One browser per test run, a fresh isolated context and page per scenario, tracing on every
/// scenario and a screenshot plus trace saved and attached to the test result when a scenario fails.
/// </summary>
[Binding]
public sealed class PlaywrightHooks
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;

    private readonly ScenarioContext _scenario;
    private readonly BrowserSession _session;

    public PlaywrightHooks(ScenarioContext scenario, BrowserSession session)
    {
        _scenario = scenario;
        _session = session;
    }

    [BeforeTestRun]
    public static async Task StartBrowserAsync()
    {
        var settings = TestSettings.Current;
        TestLog.Logger.Information(
            "Test run on {Environment}: app {BaseUrl}, API {ApiUrl}, browser {Browser}",
            settings.EnvironmentName, settings.BaseUrl, settings.ApiUrl, settings.Browser);

        _playwright = await Playwright.CreateAsync();
        var browserType = settings.Browser.ToLowerInvariant() switch
        {
            "firefox" => _playwright.Firefox,
            "webkit" => _playwright.Webkit,
            _ => _playwright.Chromium,
        };
        _browser = await browserType.LaunchAsync(new BrowserTypeLaunchOptions { Headless = settings.Headless });
    }

    [BeforeScenario]
    public async Task OpenPageAsync()
    {
        _session.Context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = TestSettings.Current.BaseUrl,
        });
        await _session.Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true,
        });
        _session.Page = await _session.Context.NewPageAsync();
        TestLog.Logger.Information("Scenario started: {Scenario}", _scenario.ScenarioInfo.Title);
    }

    [AfterStep]
    public void LogStep()
    {
        var step = _scenario.StepContext.StepInfo;
        if (_scenario.TestError is null)
        {
            TestLog.Logger.Information("  passed: {Keyword} {Step}", step.StepDefinitionType, step.Text);
        }
        else
        {
            TestLog.Logger.Error("  FAILED: {Keyword} {Step}: {Error}", step.StepDefinitionType, step.Text, _scenario.TestError.Message);
        }
    }

    [AfterScenario]
    public async Task ClosePageAsync()
    {
        var failed = _scenario.TestError is not null;
        if (failed)
        {
            var artifacts = Path.Combine(AppContext.BaseDirectory, "artifacts");
            Directory.CreateDirectory(artifacts);
            var name = SafeFileName(_scenario.ScenarioInfo.Title);
            var screenshot = Path.Combine(artifacts, $"{name}.png");
            var trace = Path.Combine(artifacts, $"{name}-trace.zip");

            await _session.Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshot, FullPage = true });
            await _session.Context.Tracing.StopAsync(new TracingStopOptions { Path = trace });
            TestContext.AddTestAttachment(screenshot, "Screenshot at failure");
            TestContext.AddTestAttachment(trace, "Playwright trace (open with: pwsh playwright.ps1 show-trace)");
            TestLog.Logger.Error("Scenario failed: {Scenario}. Screenshot and trace saved to {Folder}", _scenario.ScenarioInfo.Title, artifacts);
        }
        else
        {
            await _session.Context.Tracing.StopAsync();
            TestLog.Logger.Information("Scenario passed: {Scenario}", _scenario.ScenarioInfo.Title);
        }

        await _session.Context.CloseAsync();
    }

    [AfterTestRun]
    public static async Task StopBrowserAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
    }

    private static string SafeFileName(string title) =>
        string.Concat(title.Select(c => char.IsLetterOrDigit(c) ? c : '-')).Trim('-');
}
