namespace Microsoft.Extensions.DependencyInjection;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddCustomHealthChecks([NotNull] this IServiceCollection services)
        => services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

    public static IHealthChecksBuilder AddDapr(this IHealthChecksBuilder builder)
        => builder.AddCheck<DaprHealthCheck>("dapr");

    public static void MapCustomHealthChecks(
        this WebApplication app,
        string healthPattern = "/hc",
        string livenessPattern = "/liveness",
        Func<HttpContext, HealthReport, Task>? responseWriter = default)
    {
        app.MapHealthChecks(healthPattern, new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = responseWriter ?? UIResponseWriter.WriteHealthCheckUIResponse,
        });
        app.MapHealthChecks(livenessPattern, new HealthCheckOptions
        {
            Predicate = r => r.Name.Contains("self", StringComparison.InvariantCulture)
        });
    }

    protected sealed class DaprHealthCheck : IHealthCheck
    {
        private readonly DaprClient _daprClient;

        public DaprHealthCheck(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            [NotNull] HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var healthy = await _daprClient.CheckHealthAsync(cancellationToken).ConfigureAwait(false);

            if (healthy)
            {
                return HealthCheckResult.Healthy("Dapr sidecar is healthy.");
            }

            return new HealthCheckResult(
                context.Registration.FailureStatus,
                "Dapr sidecar is unhealthy.");
        }
    }
}
