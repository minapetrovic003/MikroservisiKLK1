namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Models
{
    public class EventEntity
    {
        public Guid Id { get; set; }

        public Guid AggregateId { get; set; }

        public string EventType { get; set; } = string.Empty;

        public string EventData { get; set; } = string.Empty;

        public int Version { get; set; }

        public DateTime OccurredOn { get; set; }
    }
}