using Serilog;

namespace Microsoft.Extensions.DependencyInjection;

public static class SerilogExtensions
{
    public static void ConfigureSerilog([NotNull] this WebApplicationBuilder builder, string appName)
    {
        var logstashUrl = builder.Configuration["Serilog:LogstashgUrl"];

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithProperty("ApplicationName", appName)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
