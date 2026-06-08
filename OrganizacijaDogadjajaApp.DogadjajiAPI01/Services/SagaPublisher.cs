using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
//Koreografija

namespace OrganizacijaDogadjajaApp.DogadjajiAPI01.Services
{
    public class SagaPublisher : ISagaPublisher
    {
        private readonly IConnection _connection;

        public SagaPublisher(IConnection connection)
        {
            _connection = connection;
        }

        public async Task PublishAsync<T>(
            T message,
            string routingKey,
            CancellationToken cancellationToken = default)
        {
            await using var channel =
                await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "saga-exchange",
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: cancellationToken);

            var json = JsonSerializer.Serialize(message);

            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: "saga-exchange",
                routingKey: routingKey,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}