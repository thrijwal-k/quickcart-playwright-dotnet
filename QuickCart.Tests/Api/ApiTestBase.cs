using System.Text.Json;
using NUnit.Framework;
using QuickCart.Tests.Support;
using RestSharp;

namespace QuickCart.Tests.Api;

/// <summary>Shared RestSharp client and helpers for the API test fixtures.</summary>
public abstract class ApiTestBase
{
    protected RestClient Client { get; private set; } = null!;

    [OneTimeSetUp]
    public void CreateClient()
    {
        Client = new RestClient(new RestClientOptions(TestSettings.Current.ApiUrl));
        TestLog.Logger.Information("API tests targeting {ApiUrl}", TestSettings.Current.ApiUrl);
    }

    [OneTimeTearDown]
    public void DisposeClient() => Client.Dispose();

    /// <summary>Reads the "errors" array from a 4xx validation response.</summary>
    protected static IReadOnlyList<string> ReadErrors(RestResponse response)
    {
        using var json = JsonDocument.Parse(response.Content ?? "{}");
        return json.RootElement.TryGetProperty("errors", out var errors)
            ? errors.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToList()
            : Array.Empty<string>();
    }

    protected static string ReadError(RestResponse response)
    {
        using var json = JsonDocument.Parse(response.Content ?? "{}");
        return json.RootElement.TryGetProperty("error", out var error) ? error.GetString() ?? string.Empty : string.Empty;
    }
}
