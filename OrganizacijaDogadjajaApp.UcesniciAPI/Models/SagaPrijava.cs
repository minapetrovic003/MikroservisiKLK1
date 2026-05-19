namespace OrganizacijaDogadjajaApp.UcesniciAPI.Models
{
    public class SagaPrijava
    {
        public Guid Id { get; set; }
        public Guid DogadjajId { get; set; }
        public Guid UcesnikId { get; set; }
        public Guid RezervacijaId { get; set; }
        public DateTime DatumPrijave { get; set; }
        public bool Otkazana { get; set; }
    }
}