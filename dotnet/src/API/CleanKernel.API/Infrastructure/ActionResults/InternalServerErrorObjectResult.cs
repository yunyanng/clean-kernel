namespace CleanKernel.API.Infrastructure.ActionResults;

internal sealed class InternalServerErrorObjectResult : ObjectResult
{
    public InternalServerErrorObjectResult(object error)
        : base(error)
        => StatusCode = StatusCodes.Status500InternalServerError;
}
