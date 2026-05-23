namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events
{
    public class PredavacPromenjenEvent : EventBase
    {
        public string NoviPredavac { get; set; } = string.Empty;
    }
}