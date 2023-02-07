namespace CleanKernel.API.Application.Behaviors;

public partial class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, [NotNull] RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        LogHandlingCommand(request.GetGenericTypeName(), request);

        var response = await next().ConfigureAwait(false);

        LogCommandHandled(request.GetGenericTypeName(), response);

        return response;
    }

    [LoggerMessage(0, LogLevel.Information, "----- Handling command {CommandName} ({Command})")]
    private partial void LogHandlingCommand(string commandName, TRequest command);

    [LoggerMessage(1, LogLevel.Information, "----- Command {CommandName} handled - response: {Response}")]
    private partial void LogCommandHandled(string commandName, TResponse response);
}
