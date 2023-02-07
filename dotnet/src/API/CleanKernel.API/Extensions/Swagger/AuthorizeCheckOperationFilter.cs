namespace CleanKernel.API.Extensions.Swagger;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    private readonly string[] _oAuthScheme;

    public AuthorizeCheckOperationFilter(params string[] oAuthScheme)
        => _oAuthScheme = oAuthScheme;

    public void Apply([NotNull] OpenApiOperation operation, [NotNull] OperationFilterContext context)
    {
        // Check for authorize attribute
        var hasAuthorize = context.MethodInfo.DeclaringType!.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                            context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (!hasAuthorize)
        {
            return;
        }

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

        var oAuthScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
        };

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [ oAuthScheme ] = _oAuthScheme
            }
        };
    }
}
