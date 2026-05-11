using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Shared.Events;
using OrganizacijaDogadjajaApp.UcesniciAPI.Data;
using OrganizacijaDogadjajaApp.UcesniciAPI.Entities;
using OrganizacijaDogadjajaApp.UcesniciAPI.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrganizacijaDogadjajaApp.UcesniciAPI.HostedServices
{
    public sealed class RabbitMqConsumerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqConsumerHostedService> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqConsumerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqConsumerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            // Dead Letter Exchange
            await _channel.ExchangeDeclareAsync(
                exchange: "dead.letter.exchange",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            // Dead Letter Queue - ovde stizu poruke koje nisu uspesno obradjene
            await _channel.QueueDeclareAsync(
                queue: "dead.letter.queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            // Vezujemo DLQ za DLX
            await _channel.QueueBindAsync(
                queue: "dead.letter.queue",
                exchange: "dead.letter.exchange",
                routingKey: "dead",
                cancellationToken: stoppingToken);

            var queueArguments = new Dictionary<string, object?>
            {
                 // quorum queue je obavezan za x-delivery-limit
                    { "x-queue-type", "quorum" },

                    { "x-dead-letter-exchange", "dead.letter.exchange" },

                    { "x-dead-letter-routing-key", "dead" },

                 // max broj retry pokusaja
                    { "x-delivery-limit", 10 }
            };

            await _channel.QueueDeclareAsync(
                queue: _options.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments,
                cancellationToken: stoppingToken);

            await _channel.QueueBindAsync(
                queue: _options.Queue,
                exchange: _options.Exchange,
                routingKey: _options.RoutingKey,
                cancellationToken: stoppingToken);

            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: _options.PrefetchCount,
                global: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) => await HandleMessageAsync(ea, stoppingToken);

            await _channel.BasicConsumeAsync(
                queue: _options.Queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation("UcesniciAPI sluša queue: {Queue}", _options.Queue);

            try { await Task.Delay(Timeout.Infinite, stoppingToken); }
            catch (OperationCanceledException) { }
        }

        private async Task HandleMessageAsync(
            BasicDeliverEventArgs ea,
            CancellationToken cancellationToken)
        {
            if (_channel is null) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<UcesniciDbContext>();

                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var eventData = JsonSerializer.Deserialize<DogadjajKreiranEvent>(body);

                if (eventData is null)
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                    return;
                }

                await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

                var messageId = ea.BasicProperties.MessageId;
                var alreadyProcessed = await db.ProcessedMessages
                    .AnyAsync(x => x.EventId == messageId, cancellationToken);

                if (!alreadyProcessed)
                {
                    var referenca = new DogadjajReferenca
                    {
                        Id = Guid.NewGuid(),
                        DogadjajId = eventData.DogadjajId,
                        NazivDogadjaja = eventData.NazivDogadjaja,
                        AgendaDogadjaja = eventData.AgendaDogadjaja,
                        DatumIVreme = eventData.DatumIVreme,
                        WelcomeMessage =
                            $"Dobrodošli na '{eventData.NazivDogadjaja}'! " +
                            $"Dogadjaj se održava " +
                            $"{eventData.DatumIVreme:dd.MM.yyyy HH:mm} " +
                            $"na lokaciji {eventData.NazivLokacije}. " +
                            $"Agenda: {eventData.AgendaDogadjaja}"
                    };

                    db.DogadjajiReference.Add(referenca);

                    db.ProcessedMessages.Add(new ProcessedMessage
                    {
                        EventId = messageId!,
                        EventType = "DogadjajKreiran",
                        ProcessedAtUtc = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync(cancellationToken);
                    await tx.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "DogadjajReferenca sacuvana za {DogadjajId}",
                        eventData.DogadjajId);
                }

                await _channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri obradi poruke. DeliveryTag: {Tag}", ea.DeliveryTag);

                if (_channel is not null)
                {
                    // requeue: false -> poruka ide u Dead Letter Queue, ne vraca se u isti red
                    await _channel.BasicNackAsync(
                        ea.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: cancellationToken);
                }
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}