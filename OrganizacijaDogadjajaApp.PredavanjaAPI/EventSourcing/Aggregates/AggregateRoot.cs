using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events;

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Aggregates
{
    public abstract class AggregateRoot
    {
        private readonly List<EventBase> _changes = new(); //Jos nisu sacuvani u bazi

        public Guid Id { get; protected set; }

        public int Version { get; protected set; }

        public IReadOnlyCollection<EventBase> GetUncommittedChanges()
        {
            //Vraca event koj nije sacuvan
            //repository -> vraca bas njih i salje u Eent Store
            return _changes.AsReadOnly();
        }

        public void MarkChangesAsCommitted()
        {
            _changes.Clear();
            //Svi eventi sacuvani -> Ocisti mi listu
        }

        protected void RaiseEvent(EventBase @event)
        {
            @event.Version = Version + 1;

            Apply(@event); //Menja state a ne stanje rucno

            _changes.Add(@event);
        }

        public void ReplayEvents(IEnumerable<EventBase> events)
        {
            //rekonstrukcija stanja
            foreach (var @event in events.OrderBy(e => e.Version))
            {
                Apply(@event); //Ponovo gradi state objekte

                Version = @event.Version; //Na kraju pamtimo poslednju verziju
            }
        }

        protected abstract void Apply(EventBase @event);
    }
}