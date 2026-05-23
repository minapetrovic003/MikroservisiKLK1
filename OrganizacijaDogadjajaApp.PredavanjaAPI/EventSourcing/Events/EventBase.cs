namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events
{
    public abstract class EventBase
    {
        public Guid EventId { get; set; } = Guid.NewGuid();

        public Guid AggregateId { get; set; }

        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;

        public int Version { get; set; }
    }
}