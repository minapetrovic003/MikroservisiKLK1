namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Services
{
    public interface ISagaPublisher
    {
        Task PublishAsync<T>(
            T message,
            string routingKey,
            CancellationToken cancellationToken = default);
    }
}