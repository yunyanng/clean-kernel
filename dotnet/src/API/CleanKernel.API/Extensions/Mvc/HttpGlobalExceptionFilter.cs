namespace CleanKernel.API.Extensions.Mvc;

public partial class HttpGlobalExceptionFilter : IExceptionFilter
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<HttpGlobalExceptionFilter> _logger;
    private readonly Type _domainExceptionType;

    public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger, Type domainExceptionType)
    {
        _env = env;
        _logger = logger;
        _domainExceptionType = domainExceptionType;
    }

    public void OnException([NotNull] ExceptionContext context)
    {
        LogError(context.Exception, context.Exception.Message);

        if (context.Exception.GetType() == _domainExceptionType)
        {
            var problemDetails = new ValidationProblemDetails()
            {
                Instance = context.HttpContext.Request.Path,
                Status = StatusCodes.Status400BadRequest,
                Detail = "Please refer to the errors property for additional details."
            };

            problemDetails.Errors.Add("DomainValidations", new string[] { context.Exception.Message.ToString() });

            context.Result = new BadRequestObjectResult(problemDetails);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        else
        {
            var json = new JsonErrorResponse
            {
                Messages = new[] { "An error occurred. Try it again." }
            };

            if (_env.IsDevelopment())
            {
                json.DeveloperMessage = context.Exception;
            }

            context.Result = new InternalServerErrorObjectResult(json);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        context.ExceptionHandled = true;
    }

    [LoggerMessage(0, LogLevel.Error, "{Message}")]
    private partial void LogError(Exception exception, string message);

    private sealed class JsonErrorResponse
    {
        [AllowNull]
        public string[] Messages { get; set; }

        public object? DeveloperMessage { get; set; }
    }
}
