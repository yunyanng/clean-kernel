namespace CleanKernel.API.Infrastructure.Idempotency;

public class IdempotencySettings
{
    public int IdempotentIdDueTime { get; set; } = 60;
}
