namespace OrganizacijaDogadjajaApp.DogadjajiAPI01.Services
    //Koreografija
{
    public interface ISagaPublisher
    {
        Task PublishAsync<T>(
            T message,
            string routingKey,
            CancellationToken cancellationToken = default);
    }
}