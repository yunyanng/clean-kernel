namespace Microsoft.Extensions.DependencyInjection;

public static class KestrelExtensions
{
    public static void ConfigureKestrel([NotNull] this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            var (httpPort, grpcPort) = GetDefinedPorts(builder.Configuration);
            options.Listen(IPAddress.Any, httpPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            });

            options.Listen(IPAddress.Any, grpcPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });
    }

    private static (int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
    {
        var grpcPort = config.GetValue("GRPC_PORT", 5001);
        var port = config.GetValue("PORT", 80);
        return (port, grpcPort);
    }
}
