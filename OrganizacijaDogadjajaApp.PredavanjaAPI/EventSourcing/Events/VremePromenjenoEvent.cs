namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events
{
    public class VremePromenjenoEvent : EventBase
    {
        public DateTime NovoVreme { get; set; }
    }
}