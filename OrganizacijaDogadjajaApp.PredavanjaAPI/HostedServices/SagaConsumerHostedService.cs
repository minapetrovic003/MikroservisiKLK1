using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using OrganizacijaDogadjajaApp.DTO.EventSaga;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Services;
//Koreografija

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.HostedServices
{
    public class SagaConsumerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger<SagaConsumerHostedService> _logger;

        private readonly IConnection _connection;

        public SagaConsumerHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<SagaConsumerHostedService> logger,
            IConnection connection)
        {
            _scopeFactory = scopeFactory;

            _logger = logger;

            _connection = connection;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var channel = await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "saga-exchange",
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: "predavanja-saga-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            //samo ovaj evet slusa
            await channel.QueueBindAsync(
                queue: "predavanja-saga-queue",
                exchange: "saga-exchange",
                routingKey: "prijava.kreirana",
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var sagaEvent =
                        JsonSerializer.Deserialize<PrijavaKreiranaEvent>(json);

                    if (sagaEvent is null)
                        return;

                    _logger.LogInformation(
                        "[SAGA {SagaId}] Primljen event prijava.kreirana",
                        sagaEvent.SagaId);

                    using var scope = _scopeFactory.CreateScope();

                    var publisher =
                        scope.ServiceProvider
                            .GetRequiredService<ISagaPublisher>();



                    // SIMULACIJA REZERVACIJE
                    //50% uspeh, 50% fail
                    var success =
                        DateTime.Now.Second % 2 == 0;
                    //var success = true;
                    //var success = false;


                    // SUCCESS

                    if (success)
                    {
                        var successEvent =
                            new PredavanjeRezervisanoEvent
                            {
                                SagaId = sagaEvent.SagaId,

                                PrijavaId = sagaEvent.PrijavaId,

                                PredavanjeId = sagaEvent.PredavanjeId,

                                ReservedAt = DateTime.UtcNow
                            };

                        await publisher.PublishAsync(
                            successEvent,
                            "predavanje.rezervisano");

                        _logger.LogInformation(
                            "[SAGA {SagaId}] Predavanje rezervisano",
                            sagaEvent.SagaId);
                    }



                    // FAILURE

                    else
                    {
                        var failedEvent =
                            new RezervacijaPredavanjaNeuspelaEvent
                            {
                                SagaId = sagaEvent.SagaId,

                                PrijavaId = sagaEvent.PrijavaId,

                                PredavanjeId = sagaEvent.PredavanjeId,

                                Reason = "Nema slobodnih mesta.",

                                FailedAt = DateTime.UtcNow
                            };

                        await publisher.PublishAsync(
                            failedEvent,
                            "predavanje.neuspesno");

                        _logger.LogInformation(
                            "[SAGA {SagaId}] Rezervacija neuspesna",
                            sagaEvent.SagaId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Greska u Saga consumer-u.");
                }
            };

            await channel.BasicConsumeAsync(
                queue: "predavanja-saga-queue",
                autoAck: true,
                consumer: consumer);

            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
    }
}