using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using OrganizacijaDogadjajaApp.DTO.EventSaga;
using OrganizacijaDogadjajaApp.UcesniciAPI.Data;
using OrganizacijaDogadjajaApp.UcesniciAPI.Services;
//Koreografija

namespace OrganizacijaDogadjajaApp.UcesniciAPI.HostedServices
{
    public class SagaCompensationHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger<SagaCompensationHostedService> _logger;

        private readonly IConnection _connection;

        public SagaCompensationHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<SagaCompensationHostedService> logger,
            IConnection connection)
        {
            _scopeFactory = scopeFactory;

            _logger = logger;

            _connection = connection;
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
                queue: "ucesnici-compensation-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                queue: "ucesnici-compensation-queue",
                exchange: "saga-exchange",
                routingKey: "predavanje.neuspesno",
                cancellationToken: stoppingToken);

            var consumer =
                new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var json =
                        Encoding.UTF8.GetString(ea.Body.ToArray());

                    var failedEvent =
                        JsonSerializer.Deserialize
                        <RezervacijaPredavanjaNeuspelaEvent>(json);

                    if (failedEvent is null)
                        return;

                    _logger.LogInformation(
                        "[SAGA {SagaId}] Primljen compensation event.",
                        failedEvent.SagaId);

                    using var scope =
                        _scopeFactory.CreateScope();

                    var dbContext =
                        scope.ServiceProvider
                            .GetRequiredService<UcesniciDbContext>();

                    var publisher =
                        scope.ServiceProvider
                            .GetRequiredService<ISagaPublisher>();


                    // PRONALAZENJE PRIJAVE

                    var prijava =
                        await dbContext.Prijave
                            .FindAsync(failedEvent.PrijavaId);

                    if (prijava is null)
                    {
                        _logger.LogWarning(
                            "[SAGA {SagaId}] Prijava nije pronadjena.",
                            failedEvent.SagaId);

                        return;
                    }


                    // ROLLBACK

                    dbContext.Prijave.Remove(prijava);

                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation(
                        "[SAGA {SagaId}] Prijava obrisana (rollback).",
                        failedEvent.SagaId);


                    // COMPENSATION EVENT

                    var cancelledEvent =
                        new PrijavaPonistenaEvent
                        {
                            SagaId = failedEvent.SagaId,

                            PrijavaId = failedEvent.PrijavaId,

                            Reason = failedEvent.Reason,

                            CancelledAt = DateTime.UtcNow
                        };

                    await publisher.PublishAsync(
                        cancelledEvent,
                        "prijava.ponistena");

                    _logger.LogInformation(
                        "[SAGA {SagaId}] Poslat prijava.ponistena",
                        failedEvent.SagaId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Greska u compensation consumer-u.");
                }
            };

            await channel.BasicConsumeAsync(
                queue: "ucesnici-compensation-queue",
                autoAck: true,
                consumer: consumer);

            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
    }
}