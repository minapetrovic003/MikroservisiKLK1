namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events
{
    public class SalaPromenjenaEvent : EventBase
    {
        public string NovaSala { get; set; } = string.Empty;
    }
}