namespace OrganizacijaDogadjajaApp.UcesniciAPI.Services
{
    public interface ISagaPublisher //Sta publisher mora da zna da odradi
    {
        Task PublishAsync<T>(
            T message,
            string routingKey,//Kom eventu ide 
            CancellationToken cancellationToken = default);
    }
}