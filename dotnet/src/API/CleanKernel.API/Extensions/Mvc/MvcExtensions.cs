namespace Microsoft.Extensions.DependencyInjection;

public static class MvcExtensions
{
    public static IServiceCollection AddCustomMvc(this IServiceCollection services, Assembly assembly)
    {
        // Add framework services.
        services.AddControllers(options =>
        {
            options.Filters.Add(typeof(HttpGlobalExceptionFilter));
        })
            // Added for functional tests
            .AddApplicationPart(assembly)
            .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        return services;
    }
}
