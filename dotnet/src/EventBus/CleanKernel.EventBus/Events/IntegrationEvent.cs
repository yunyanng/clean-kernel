namespace CleanKernel.EventBus.Events;

public record IntegrationEvent
{
    public Guid Id { get; private init; }

    public DateTime CreationDate { get; private init; }

    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }
}
