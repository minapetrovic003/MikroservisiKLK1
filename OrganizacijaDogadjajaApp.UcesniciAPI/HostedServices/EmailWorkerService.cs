using Microsoft.Extensions.Options;
using OrganizacijaDogadjajaApp.UcesniciAPI.Models;
using OrganizacijaDogadjajaApp.UcesniciAPI.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrganizacijaDogadjajaApp.UcesniciAPI.HostedServices;

// Worker koji cita email queue i "salje" mejlove (zapisuje .txt fajlove)
// Rate limit: max 10 mejlova u minuti
public sealed class EmailWorkerService : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<EmailWorkerService> _logger;

    // Rate limiting state
    private int _poslatihUTekucemMinutu = 0;
    private DateTime _pocetakMinuta = DateTime.UtcNow;
    private const int MaxEmailaPoMinutu = 10;

    private IConnection? _connection;
    private IChannel? _channel;

    public EmailWorkerService(
        IOptions<RabbitMqOptions> options,
        ILogger<EmailWorkerService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Kreiramo outbox folder ako ne postoji
        var outboxPath = Path.Combine(Directory.GetCurrentDirectory(), "outbox");
        Directory.CreateDirectory(outboxPath);

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: "email.outbox",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        // Uzimamo po jednu poruku - jer kontrolisemo tempo sami
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) => await HandleEmailAsync(ea, outboxPath, stoppingToken);

        await _channel.BasicConsumeAsync(
            queue: "email.outbox",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("EmailWorker pokrenut. Max {Max} mejlova/min.", MaxEmailaPoMinutu);

        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { }
    }

    private async Task HandleEmailAsync(BasicDeliverEventArgs ea, string outboxPath, CancellationToken ct)
    {
        if (_channel is null) return;

        try
        {
            // RATE LIMITING LOGIKA
            // Proveravamo da li smo u istom minutu
            var sada = DateTime.UtcNow;
            if ((sada - _pocetakMinuta).TotalMinutes >= 1.0)
            {
                // Novi minut - resetujemo brojac
                _pocetakMinuta = sada;
                _poslatihUTekucemMinutu = 0;
            }

            if (_poslatihUTekucemMinutu >= MaxEmailaPoMinutu)
            {
                // Dostigli smo limit - cekamo do pocetka sledeceg minuta
                var preostaloMs = (int)(60_000 - (sada - _pocetakMinuta).TotalMilliseconds);
                _logger.LogWarning("Rate limit dostignut ({Max}/min). Cekam {Sec}s.",
                    MaxEmailaPoMinutu, preostaloMs / 1000);

                await Task.Delay(Math.Max(preostaloMs, 1000), ct);

                // Resetujemo posle cekanja
                _pocetakMinuta = DateTime.UtcNow;
                _poslatihUTekucemMinutu = 0;
            }

            // Citamo email iz poruke
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var email = JsonSerializer.Deserialize<EmailMessage>(body);

            if (email is null)
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
                return;
            }

            // "Saljemo" email - pisemo .txt fajl u outbox folder
            var fileName = $"email_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.txt";
            var filePath = Path.Combine(outboxPath, fileName);
            var sadrzaj = $"""
                To: {email.To}
                Subject: {email.Subject}
                Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
                
                {email.Body}
                """;

            await File.WriteAllTextAsync(filePath, sadrzaj, ct);
            _poslatihUTekucemMinutu++;

            _logger.LogInformation("Email sacuvan: {File} ({Count}/{Max} u ovom minutu)",
                fileName, _poslatihUTekucemMinutu, MaxEmailaPoMinutu);

            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri obradi email poruke.");
            if (_channel is not null)
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: ct);
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}