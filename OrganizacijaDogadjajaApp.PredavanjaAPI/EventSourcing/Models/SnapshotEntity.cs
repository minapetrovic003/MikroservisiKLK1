namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Snapshots
{
    public class SnapshotEntity
    {
        public Guid Id { get; set; }

        public Guid AggregateId { get; set; }

        public string SnapshotData { get; set; } = string.Empty;

        public int Version { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}