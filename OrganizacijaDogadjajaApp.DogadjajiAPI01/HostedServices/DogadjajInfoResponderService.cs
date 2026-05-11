using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;
using OrganizacijaDogadjajaApp.DTO.Shared.RequestReply;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.HostedServices;

// Ovaj servis slusa zahteve za informacije o dogadjaju i odgovara
public sealed class DogadjajInfoResponderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<DogadjajInfoResponderService> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public DogadjajInfoResponderService(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<DogadjajInfoResponderService> logger)
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

        // Deklarisemo request queue - ovde stizu zahtevi od UcesniciAPI
        await _channel.QueueDeclareAsync(
            queue: _options.DogadjajInfoRequestQueue,//name 
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) => await HandleRequestAsync(ea, stoppingToken);

        await _channel.BasicConsumeAsync(
            queue: _options.DogadjajInfoRequestQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("DogadjajiAPI slusa request-reply zahteve na: {Queue}",
            _options.DogadjajInfoRequestQueue);

        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { }
    }

    private async Task HandleRequestAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        if (_channel is null) return;

        try
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var request = JsonSerializer.Deserialize<DogadjajInfoRequest>(body);

            DogadjajInfoResponse response;

            if (request is null)
            {
                response = new DogadjajInfoResponse { Pronadjen = false };
            }
            else
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DogadjajiDbContext>();

                var dogadjaj = await db.Dogadjaji
                    .FirstOrDefaultAsync(d => d.Id == request.DogadjajId, cancellationToken);

                response = dogadjaj is not null
                    ? new DogadjajInfoResponse
                    {
                        DogadjajId = dogadjaj.Id,
                        NazivDogadjaja = dogadjaj.NazivDogadjaja,
                        AgendaDogadjaja = dogadjaj.AgendaDogadjaja,
                        DatumIVreme = dogadjaj.DatumIVreme,
                        Trajanje = dogadjaj.Trajanje,
                        Pronadjen = true
                    }
                    : new DogadjajInfoResponse { DogadjajId = request.DogadjajId, Pronadjen = false };
            }

            // Saljemo odgovor nazad na ReplyTo queue (kao u vezbi!)
            if (!string.IsNullOrWhiteSpace(ea.BasicProperties.ReplyTo))
            {
                var responseJson = JsonSerializer.Serialize(response);
                var responseBody = Encoding.UTF8.GetBytes(responseJson);

                var props = new BasicProperties
                {
                    CorrelationId = ea.BasicProperties.CorrelationId
                };

                await _channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: ea.BasicProperties.ReplyTo,
                    mandatory: false,
                    basicProperties: props,
                    body: responseBody,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Odgovor na request-reply poslat. CorrelationId: {Id}",
                    ea.BasicProperties.CorrelationId);
            }

            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška u request-reply handleru.");
            await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false,
                cancellationToken: cancellationToken);
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}