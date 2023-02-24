namespace Microsoft.Extensions.DependencyInjection;

public static partial class ExceptionExtensions
{
    public static void AddExceptionJsonConverter(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new ExceptionJsonConverter<Exception>());
        });
    }

    public static void UseExceptionHandler<TDomainException>(this WebApplication app)
        where TDomainException : Exception
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                if (exceptionHandlerFeature is null)
                {
                    return;
                }

                var logger = context.Resolve<ILogger<ExceptionHandler>>();

                LogError(logger, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);

                if (exceptionHandlerFeature.Error.GetType() is TDomainException)
                {
                    var problemDetails = new ValidationProblemDetails()
                    {
                        Instance = exceptionHandlerFeature.Path,
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Please refer to the errors property for additional details."
                    };

                    problemDetails.Errors.Add("DomainValidations", new string[] { exceptionHandlerFeature.Error.Message.ToString() });

                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    await context.Response.WriteAsJsonAsync(new BadRequestObjectResult(problemDetails)).ConfigureAwait(false);
                }
                else
                {
                    var json = new JsonErrorResponse
                    {
                        Messages = new[] { "An error occurred. Try it again." }
                    };

                    if (app.Environment.IsDevelopment())
                    {
                        json.DeveloperMessage = exceptionHandlerFeature.Error;
                    }

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    await context.Response.WriteAsJsonAsync(new InternalServerErrorObjectResult(json)).ConfigureAwait(false);
                }
            });
        });
    }

    [LoggerMessage(0, LogLevel.Error, "{Message}")]
    private static partial void LogError(ILogger<ExceptionHandler> logger, Exception exception, string message);

    private sealed class JsonErrorResponse
    {
        [AllowNull]
        public string[] Messages { get; set; }

        public object? DeveloperMessage { get; set; }
    }

    private sealed class ExceptionHandler
    {
        public ExceptionHandler _discard = new();
    }
}
