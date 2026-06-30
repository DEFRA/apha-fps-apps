namespace Apha.Common.Utilities.EventPublisher
{
    public interface IEventPublisherService
    {
        public Task<string> PublishAsync(BatchTriggerEventDetail detail, CancellationToken cancellationToken = default);
    }
}
