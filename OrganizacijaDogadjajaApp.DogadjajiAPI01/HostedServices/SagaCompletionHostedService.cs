using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using OrganizacijaDogadjajaApp.DTO.EventSaga;
using OrganizacijaDogadjajaApp.DogadjajiAPI01.Services;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI01.HostedServices
{
    public class SagaCompletionHostedService : BackgroundService
    {
        private readonly ILogger<SagaCompletionHostedService> _logger;

        private readonly IConnection _connection;

        private readonly IServiceScopeFactory _scopeFactory;

        public SagaCompletionHostedService(
            ILogger<SagaCompletionHostedService> logger,
            IConnection connection,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;

            _connection = connection;

            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var channel =
                await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "saga-exchange",
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: "dogadjaji-saga-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                queue: "dogadjaji-saga-queue",
                exchange: "saga-exchange",
                routingKey: "predavanje.rezervisano",
                cancellationToken: stoppingToken);

            var consumer =
                new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var json =
                        Encoding.UTF8.GetString(ea.Body.ToArray());

                    var successEvent =
                        JsonSerializer.Deserialize
                        <PredavanjeRezervisanoEvent>(json);

                    if (successEvent is null)
                        return;

                    _logger.LogInformation(
                        "[SAGA {SagaId}] Primljen success event.",
                        successEvent.SagaId);

                    using var scope =
                        _scopeFactory.CreateScope();

                    var publisher =
                        scope.ServiceProvider
                            .GetRequiredService<ISagaPublisher>();



                    // FINALNI EVENT

                    var completedEvent =
                        new SagaZavrsenaEvent
                        {
                            SagaId = successEvent.SagaId,

                            PrijavaId = successEvent.PrijavaId,

                            CompletedAt = DateTime.UtcNow
                        };

                    await publisher.PublishAsync(
                        completedEvent,
                        "saga.zavrsena");



                    // LOGOVANJE

                    _logger.LogInformation(
                        "[SAGA {SagaId}] Saga uspesno zavrsena.",
                        successEvent.SagaId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Greska u Saga completion consumer-u.");
                }
            };

            await channel.BasicConsumeAsync(
                queue: "dogadjaji-saga-queue",
                autoAck: true,
                consumer: consumer);

            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
    }
}