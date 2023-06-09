using Serilog;

namespace Microsoft.Extensions.DependencyInjection;

public static class SerilogExtensions
{
    public static void ConfigureSerilog([NotNull] this WebApplicationBuilder builder, string appName)
    {
        var fluentBitUrl = builder.Configuration["Serilog:FluentBitUrl"];

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithProperty("ApplicationName", appName)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.Http(string.IsNullOrWhiteSpace(fluentBitUrl) ? $"http://fluent-bit:9880/{WebUtility.UrlEncode(appName)}" : fluentBitUrl, null)
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
