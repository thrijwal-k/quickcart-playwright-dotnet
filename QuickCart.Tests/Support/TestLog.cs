using Serilog;

namespace QuickCart.Tests.Support;

/// <summary>Structured logging for the test run: console output for CI plus a daily log file.</summary>
public static class TestLog
{
    private static readonly Lazy<ILogger> Instance = new(() => new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            Path.Combine(AppContext.BaseDirectory, "logs", "test-run-.log"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger());

    public static ILogger Logger => Instance.Value;
}
