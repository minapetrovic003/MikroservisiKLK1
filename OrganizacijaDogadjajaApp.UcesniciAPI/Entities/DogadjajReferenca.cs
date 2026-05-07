namespace OrganizacijaDogadjajaApp.UcesniciAPI.Entities
{
    public class DogadjajReferenca
    {
        public Guid Id { get; set; }
        public Guid DogadjajId { get; set; }
        public string NazivDogadjaja { get; set; } = string.Empty;
        public string AgendaDogadjaja { get; set; } = string.Empty;
        public DateTime DatumIVreme { get; set; }
        public string WelcomeMessage { get; set; } = string.Empty;
    }
}