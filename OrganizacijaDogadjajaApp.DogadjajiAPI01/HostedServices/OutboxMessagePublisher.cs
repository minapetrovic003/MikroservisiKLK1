using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Services;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.HostedServices
{
    //Proverava OutBox i salje poruke RabbitMq
    
    public class OutboxMessagePublisher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxMessagePublisher> _logger;

        public OutboxMessagePublisher(IServiceScopeFactory scopeFactory, ILogger<OutboxMessagePublisher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DogadjajiDbContext>();
                    var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

                   
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