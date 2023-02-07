namespace Microsoft.Extensions.DependencyInjection;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwagger(
        this IServiceCollection services,
        string title,
        string description,
#pragma warning disable CA1054 // URI-like parameters should not be strings
        string identityUrlExternal,
#pragma warning restore CA1054 // URI-like parameters should not be strings
        Dictionary<string, string> scopes)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>(
            serviceProvider => new(
                serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>(),
                title,
                description));

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{identityUrlExternal}/connect/authorize"),
                        TokenUrl = new Uri($"{identityUrlExternal}/connect/token"),
                        Scopes = scopes
                    }
                }
            });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
            options.OperationFilter<SwaggerDefaultValues>();
        });

        return services;
    }
}
