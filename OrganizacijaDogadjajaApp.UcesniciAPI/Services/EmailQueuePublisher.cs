using Microsoft.Extensions.Options;
using OrganizacijaDogadjajaApp.UcesniciAPI.Models;
using OrganizacijaDogadjajaApp.UcesniciAPI.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrganizacijaDogadjajaApp.UcesniciAPI.Services;

public interface IEmailQueuePublisher
{
    Task StaviURedAsync(EmailMessage email, CancellationToken ct = default);
}

public sealed class EmailQueuePublisher : IEmailQueuePublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public EmailQueuePublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task StaviURedAsync(EmailMessage email, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(email));
        var props = new BasicProperties { Persistent = true };

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "email.outbox",
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);
    }

    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (_channel is not null) return;
        await _lock.WaitAsync(ct);
        try
        {
            if (_channel is not null) return;
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };
            _connection = await factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
            await _channel.QueueDeclareAsync(
                queue: "email.outbox",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct);
        }
        finally { _lock.Release(); }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        _lock.Dispose();
    }
}