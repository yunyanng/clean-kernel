namespace CleanKernel.API.Infrastructure.Idempotency;

public interface IIdempotencyActor : IActor
{
    Task<bool> ExistAsync();

    Task SetExistAsync();
}
