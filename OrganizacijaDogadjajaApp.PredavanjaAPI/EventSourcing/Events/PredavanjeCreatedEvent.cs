namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events
{
    public class PredavanjeCreatedEvent : EventBase
    {
        public string Naziv { get; set; } = string.Empty;

        public string Predavac { get; set; } = string.Empty;

        public string Sala { get; set; } = string.Empty;

        public DateTime VremeOdrzavanja { get; set; }
    }
}