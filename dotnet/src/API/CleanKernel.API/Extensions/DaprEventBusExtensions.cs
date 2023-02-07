namespace Microsoft.Extensions.DependencyInjection;

public static class DaprEventBusExtensions
{
    public static void AddScopedDaprEventBus(this IServiceCollection services, string pubSubName)
        => services.AddScoped<IEventBus, DaprEventBus>(serviceProvider
            => new(
                pubSubName,
                serviceProvider.GetRequiredService<DaprClient>(),
                serviceProvider.GetRequiredService<ILogger<DaprEventBus>>()));
}
