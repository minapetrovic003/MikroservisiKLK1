namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events
{
    public class NazivPromenjenEvent : EventBase
    {
        public string NoviNaziv { get; set; } = string.Empty;
    }
}