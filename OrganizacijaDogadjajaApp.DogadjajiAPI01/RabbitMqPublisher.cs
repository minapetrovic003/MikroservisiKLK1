using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Services
{
    
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(string payload, string messageId, string eventType, CancellationToken cancellationToken);
    }

    public sealed class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
    {
        //Sealed -> klasa ne moze da se nasledi 
        private readonly ConnectionFactory _factory;
        private readonly RabbitMqOptions _options;

        private readonly SemaphoreSlim _initLock = new(1, 1);

        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqPublisher(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;

            _factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };
        }

        public async Task PublishAsync(string payload, string messageId, string eventType, CancellationToken cancellationToken)
        {
            // Lazy inicijalizacija - konekcija se pravi tek kad je treba
            await EnsureInitializedAsync(cancellationToken);

            if (_channel is null)
                throw new InvalidOperationException("RabbitMQ channel nije inicijalizovan.");

            var body = Encoding.UTF8.GetBytes(payload);

            var properties = new BasicProperties
            {
                
                Persistent = true,          // poruka prezivi restart RabbitMQ-a
                MessageId = messageId,      // jedinstveni ID - koriste consumeri za idempotentnost
                Type = eventType,           // tip eventa (npr. "DogadjajKreiran")
                ContentType = "application/json"
            };

            await _channel.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: "",             
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) return;

            await _initLock.WaitAsync(cancellationToken);
            try
            {
                if (_channel is not null) return;

                _connection = await _factory.CreateConnectionAsync(cancellationToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

                
                await _channel.ExchangeDeclareAsync(
                    exchange: _options.Exchange,
                    type: ExchangeType.Fanout,  
                    durable: true,
                    autoDelete: false,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null) await _channel.DisposeAsync();
            if (_connection is not null) await _connection.DisposeAsync();
            _initLock.Dispose();
        }
    }
}