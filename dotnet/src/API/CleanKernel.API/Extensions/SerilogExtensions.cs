using Serilog;

namespace Microsoft.Extensions.DependencyInjection;

public static class SerilogExtensions
{
    public static void ConfigureSerilog([NotNull] this WebApplicationBuilder builder, string appName)
    {
        var fluentdUrl = builder.Configuration["Serilog:FluentdUrl"];

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithProperty("ApplicationName", appName)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.Http(string.IsNullOrWhiteSpace(fluentdUrl) ? $"http://fluentd:9880/{WebUtility.UrlEncode(appName)}" : fluentdUrl, null)
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
