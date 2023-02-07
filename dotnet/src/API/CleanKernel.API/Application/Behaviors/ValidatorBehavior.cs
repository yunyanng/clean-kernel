namespace CleanKernel.API.Application.Behaviors;

public partial class ValidatorBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidatorBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly Func<string, ValidationException, Exception> _newDomainException;

    public ValidatorBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidatorBehavior<TRequest, TResponse>> logger,
        Func<string, ValidationException, Exception> newDomainException)
    {
        _validators = validators;
        _logger = logger;
        _newDomainException = newDomainException;
    }

    public async Task<TResponse> Handle(TRequest request, [NotNull] RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var typeName = request.GetGenericTypeName();

        LogValidatingCommand(typeName);

        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        if (failures.Any())
        {
            LogValidationErrors(typeName, request, failures);

            var domainException = _newDomainException(
                $"Command Validation Errors for type {typeof(TRequest).Name}",
                new ValidationException("Validation exception", failures));

            throw domainException;
        }

        return await next().ConfigureAwait(false);
    }

    [LoggerMessage(0, LogLevel.Information, "----- Validating command {CommandType}")]
    private partial void LogValidatingCommand(string commandType);

    [LoggerMessage(1, LogLevel.Warning, "Validation errors - {CommandType} - Command: {Command} - Errors: {ValidationErrors}")]
    private partial void LogValidationErrors(string commandType, TRequest command, List<ValidationFailure> validationErrors);
}
