using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Aggregates;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Models;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Snapshots;

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Repositories
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly EventStoreDbContext _context;

        public EventStoreRepository(EventStoreDbContext context)
        {
            _context = context;
        }

        public async Task SaveEventsAsync(
            Guid aggregateId,
            IEnumerable<EventBase> events)
        {
            foreach (var @event in events)
            {
                var eventEntity = new EventEntity
                {
                    Id = Guid.NewGuid(),
                    AggregateId = aggregateId,
                    EventType = @event.GetType().AssemblyQualifiedName!,
                    EventData = JsonSerializer.Serialize(@event),
                    Version = @event.Version,
                    OccurredOn = @event.OccurredOn
                };

                await _context.Events.AddAsync(eventEntity);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<EventBase>> GetEventsAsync(Guid aggregateId)
        {
            var eventEntities = await _context.Events
                .Where(x => x.AggregateId == aggregateId)
                .OrderBy(x => x.Version)
                .ToListAsync();

            var events = new List<EventBase>();

            foreach (var entity in eventEntities)
            {
                var type = Type.GetType(entity.EventType);

                if (type == null)
                    continue;

                var @event = JsonSerializer.Deserialize(
                    entity.EventData,
                    type) as EventBase;

                if (@event != null)
                {
                    events.Add(@event);
                }
            }

            return events;
        }

        public async Task SaveSnapshotAsync(
            Guid aggregateId,
            object snapshot,
            int version)
        {
            var snapshotEntity = new SnapshotEntity
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                SnapshotData = JsonSerializer.Serialize(snapshot),
                Version = version,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Snapshots.AddAsync(snapshotEntity);

            await _context.SaveChangesAsync();
        }

        public async Task<T?> GetLatestSnapshotAsync<T>(Guid aggregateId)
        {
            var snapshot = await _context.Snapshots
                .Where(x => x.AggregateId == aggregateId)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync();

            if (snapshot == null)
                return default;

            return JsonSerializer.Deserialize<T>(
                snapshot.SnapshotData);
        }

        public async Task<PredavanjeAggregate?> LoadAggregateAsync(
    Guid aggregateId)
        {
            var aggregate = new PredavanjeAggregate();

            var snapshot =
                await GetLatestSnapshotAsync<PredavanjeSnapshot>(
                    aggregateId);

            int snapshotVersion = 0;

            if (snapshot != null)
            {
                aggregate.RestoreFromSnapshot(snapshot);

                snapshotVersion = snapshot.Version;
            }

            var eventEntities = await _context.Events
                .Where(x =>
                    x.AggregateId == aggregateId &&
                    x.Version > snapshotVersion)
                .OrderBy(x => x.Version)
                .ToListAsync();

            var events = new List<EventBase>();

            foreach (var entity in eventEntities)
            {
                var type = Type.GetType(entity.EventType);

                if (type == null)
                    continue;

                var @event = JsonSerializer.Deserialize(
                    entity.EventData,
                    type) as EventBase;

                if (@event != null)
                {
                    events.Add(@event);
                }
            }

            if (!events.Any() && snapshot == null)
                return null;

            aggregate.ReplayEvents(events);

            return aggregate;
        }
    }
}