namespace CleanKernel.API.Infrastructure.Idempotency;

public class IdempotencyActor : Actor, IIdempotencyActor, IRemindable
{
    private const string ExistStateName = "exist";
    private const string ClearStateReminderName = "clear-state";
    private readonly IOptions<IdempotencySettings> _settings;

    public IdempotencyActor(
        ActorHost host,
        IOptions<IdempotencySettings> settings)
        : base(host)
    {
        _settings = settings;
    }

    public Task<bool> ExistAsync()
        => StateManager.ContainsStateAsync(ExistStateName);

    public async Task SetExistAsync()
    {
        await RegisterReminderAsync(
            ClearStateReminderName,
            Array.Empty<byte>(),
            TimeSpan.FromMinutes(_settings.Value.IdempotentIdDueTime),
            TimeSpan.FromTicks(-1))
            .ConfigureAwait(false);

        await StateManager
            .SetStateAsync(ExistStateName, true)
            .ConfigureAwait(false);
    }

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        await StateManager.TryRemoveStateAsync(ExistStateName).ConfigureAwait(false);
        await UnregisterReminderAsync(ClearStateReminderName).ConfigureAwait(false);
    }
}
