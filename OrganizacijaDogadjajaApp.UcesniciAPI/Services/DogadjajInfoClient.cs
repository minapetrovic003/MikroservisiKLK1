using Microsoft.Extensions.Options;
using OrganizacijaDogadjajaApp.DTO.Shared.RequestReply;
using OrganizacijaDogadjajaApp.UcesniciAPI.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace OrganizacijaDogadjajaApp.UcesniciAPI.Services;

// Ovaj klijent implementira request-reply - isto kao MolbaRequestReplyClient iz vezbi!
public sealed class DogadjajInfoClient : IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<DogadjajInfoClient> _logger;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<DogadjajInfoResponse>> _pending = new();

    private IConnection? _connection;
    private IChannel? _publishChannel;
    private IChannel? _consumerChannel;

    public DogadjajInfoClient(IOptions<RabbitMqOptions> options, ILogger<DogadjajInfoClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _publishChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        _consumerChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Deklarisemo reply queue
        await _consumerChannel.QueueDeclareAsync(
            queue: _options.DogadjajInfoReplyQueue,
            durable: false,
            exclusive: true,
            autoDelete: true,
            arguments: null,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.ReceivedAsync += HandleReplyAsync;

        await _consumerChannel.BasicConsumeAsync(
            queue: _options.DogadjajInfoReplyQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }

    // Asinhrono pita DogadjajiAPI za info, ceka odgovor
    public async Task<DogadjajInfoResponse?> GetDogadjajInfoAsync(
        Guid dogadjajId,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (_publishChannel is null)
            throw new InvalidOperationException("Klijent nije inicijalizovan.");

        var correlationId = Guid.NewGuid().ToString("N");
        var tcs = new TaskCompletionSource<DogadjajInfoResponse>();
        _pending[correlationId] = tcs;

        var request = new DogadjajInfoRequest { DogadjajId = dogadjajId };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));

        var props = new BasicProperties
        {
            CorrelationId = correlationId,
            ReplyTo = _options.DogadjajInfoReplyQueue
        };

        await _publishChannel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _options.DogadjajInfoRequestQueue,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Request-reply poslan za dogadjaj {Id}. CorrelationId: {CorrId}",
            dogadjajId, correlationId);

        // Cekamo odgovor (max 5 sekundi)
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(5));
        cts.Token.Register(() => tcs.TrySetCanceled());

        try
        {
            return await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            _pending.TryRemove(correlationId, out _);
            _logger.LogWarning("Request-reply timeout za dogadjaj {Id}.", dogadjajId);
            return null;
        }
    }

    //Mapiranje odgovora
    private async Task HandleReplyAsync(object sender, BasicDeliverEventArgs ea)
    {
        if (_consumerChannel is null) return;

        try
        {
            var correlationId = ea.BasicProperties.CorrelationId;
            if (!string.IsNullOrWhiteSpace(correlationId) &&
                _pending.TryRemove(correlationId, out var tcs))
            {
                var response = JsonSerializer.Deserialize<DogadjajInfoResponse>(ea.Body.ToArray());
                if (response is not null)
                    tcs.TrySetResult(response);
            }
        }
        finally
        {
            await _consumerChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_consumerChannel is not null) await _consumerChannel.DisposeAsync();
        if (_publishChannel is not null) await _publishChannel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}