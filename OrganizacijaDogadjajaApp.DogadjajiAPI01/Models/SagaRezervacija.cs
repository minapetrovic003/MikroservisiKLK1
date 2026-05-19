namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Models
{
    /// <summary>
    /// Rezervacija mesta na dogadjaju – kreirana od strane Saga Orchestratora.
    /// Odvojena od glavnog modela prijave jer je privremena dok se Saga ne završi.
    /// </summary>
    public class SagaRezervacija
    {
        public Guid Id { get; set; }
        public Guid DogadjajId { get; set; }
        public Guid UcesnikId { get; set; }
        public DateTime KreiranaU { get; set; }
        public bool Otkazana { get; set; }
    }
}