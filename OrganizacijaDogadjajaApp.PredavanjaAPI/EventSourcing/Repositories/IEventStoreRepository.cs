using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Aggregates;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events;

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Repositories
{
    public interface IEventStoreRepository
    {
        Task SaveEventsAsync(
            Guid aggregateId,
            IEnumerable<EventBase> events);

        Task<List<EventBase>> GetEventsAsync(Guid aggregateId);

        Task SaveSnapshotAsync(
            Guid aggregateId,
            object snapshot,
            int version);

        Task<T?> GetLatestSnapshotAsync<T>(Guid aggregateId);

        Task<PredavanjeAggregate?> LoadAggregateAsync(Guid aggregateId);
    }
}