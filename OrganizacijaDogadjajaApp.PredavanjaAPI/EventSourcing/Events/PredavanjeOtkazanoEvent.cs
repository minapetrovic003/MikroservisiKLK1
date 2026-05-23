namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events
{
    public class PredavanjeOtkazanoEvent : EventBase
    {
        public string Razlog { get; set; } = string.Empty;
    }
}