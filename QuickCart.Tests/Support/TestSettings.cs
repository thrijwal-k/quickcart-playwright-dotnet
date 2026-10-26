using System.Text.Json;

namespace QuickCart.Tests.Support;

/// <summary>
/// Environment switching: settings come from config/{TEST_ENV}.json (local, sit, uat),
/// and BASE_URL / API_URL / BROWSER environment variables override them in a pipeline.
/// </summary>
public sealed class TestSettings
{
    public string EnvironmentName { get; init; } = "local";
    public string BaseUrl { get; init; } = "http://localhost:5173";
    public string ApiUrl { get; init; } = "http://localhost:3001";
    public string Browser { get; init; } = "chromium";
    public bool Headless { get; init; } = true;

    private static readonly Lazy<TestSettings> Instance = new(Load);

    public static TestSettings Current => Instance.Value;

    private static TestSettings Load()
    {
        var env = System.Environment.GetEnvironmentVariable("TEST_ENV") ?? "local";
        var path = Path.Combine(AppContext.BaseDirectory, "config", $"{env}.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"No settings file for test environment '{env}'", path);
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var fromFile = JsonSerializer.Deserialize<TestSettings>(File.ReadAllText(path), options)
                       ?? throw new InvalidOperationException($"Could not read settings from {path}");

        return new TestSettings
        {
            EnvironmentName = env,
            BaseUrl = System.Environment.GetEnvironmentVariable("BASE_URL") ?? fromFile.BaseUrl,
            ApiUrl = System.Environment.GetEnvironmentVariable("API_URL") ?? fromFile.ApiUrl,
            Browser = System.Environment.GetEnvironmentVariable("BROWSER") ?? fromFile.Browser,
            Headless = fromFile.Headless,
        };
    }
}
