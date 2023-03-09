namespace CleanKernel.API.Infrastructure.Idempotency;

public static class IdempotencyActorExtensions
{
    public static IIdempotencyActor CreateIdempotencyActor(
        [NotNull] this IActorProxyFactory proxyFactory,
        Guid requestId)
        => proxyFactory.CreateActorProxy<IIdempotencyActor>(new(requestId.ToString()), nameof(IdempotencyActor));
}
