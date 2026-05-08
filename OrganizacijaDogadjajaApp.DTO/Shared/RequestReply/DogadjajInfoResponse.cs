namespace OrganizacijaDogadjajaApp.DTO.Shared.RequestReply
{
    public class DogadjajInfoResponse
    {
        public Guid DogadjajId { get; set; }
        public string NazivDogadjaja { get; set; } = string.Empty;
        public string AgendaDogadjaja { get; set; } = string.Empty;
        public DateTime DatumIVreme { get; set; }
        public int Trajanje { get; set; }
        public bool Pronadjen { get; set; }  // Dogadaj ne postojim= false
    }
}
