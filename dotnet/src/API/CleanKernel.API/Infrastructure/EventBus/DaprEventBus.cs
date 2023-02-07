namespace CleanKernel.API.Infrastructure.EventBus;

public partial class DaprEventBus : IEventBus
{
    private readonly string _pubSubName;
    private readonly DaprClient _dapr;
    private readonly ILogger _logger;

    public DaprEventBus(string pubSubName, DaprClient dapr, ILogger<DaprEventBus> logger)
    {
        _pubSubName = pubSubName;
        _dapr = dapr;
        _logger = logger;
    }

    public async Task PublishAsync([NotNull] IntegrationEvent integrationEvent)
    {
        var topicName = integrationEvent.GetType().Name;

        LogPublishingEvent(integrationEvent, _pubSubName, topicName);

        // We need to make sure that we pass the concrete type to PublishEventAsync,
        // which can be accomplished by casting the event to dynamic. This ensures
        // that all event fields are properly serialized.
        await _dapr.PublishEventAsync(_pubSubName, topicName, (object)integrationEvent).ConfigureAwait(false);
    }

    [LoggerMessage(0, LogLevel.Information, "Publishing event {Event} to {PubsubName}.{TopicName}")]
    private partial void LogPublishingEvent(IntegrationEvent @event, string pubsubName, string topicName);
}
