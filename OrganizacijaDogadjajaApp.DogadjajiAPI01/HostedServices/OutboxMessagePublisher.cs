using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Services;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.HostedServices
{
    // BackgroundService 
    //Proverava OutBox i salje poruke RabbitMq
    //Koristim ga jer je DB Scope a OutBox Singlton kako bi mogli komunicirti
    public class OutboxMessagePublisher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxMessagePublisher> _logger;

        // Koristimo IServiceScopeFactory jer DbContext nije Singleton
        public OutboxMessagePublisher(IServiceScopeFactory scopeFactory, ILogger<OutboxMessagePublisher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Vrtimo petlju sve dok se app ne ugasi
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DogadjajiDbContext>();
                    var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

                    // Uzimamo max 5 najstarijih poruka iz outbox tabele
                    var pending = await db.OutboxMessages
                        .OrderBy(x => x.CreatedAt)
                        .Take(5)
                        .ToListAsync(stoppingToken);

                    foreach (var message in pending)
                    {
                        try
                        {
                            await publisher.PublishAsync(
                                message.Payload,
                                message.Id.ToString(),
                                message.EventType,
                                stoppingToken);

                           
                            db.OutboxMessages.Remove(message);
                            await db.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation("Outbox poruka {Id} uspesno poslata.", message.Id);
                        }
                        catch (Exception ex)
                        {
                            
                            _logger.LogWarning(ex, "Greška pri slanju outbox poruke {Id}.", message.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Neocekivana greška u OutboxMessagePublisher.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}