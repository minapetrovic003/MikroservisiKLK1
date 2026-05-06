namespace OrganizacijaDogadjajaApp.DTO
{
    public class PredavanjeDTO
    {
        public Guid Id { get; set; }
        public string Tema { get; set; }
        public int TrajanjePredavanja { get; set; }
        public DateTime Pocetak { get; set; }
        public Guid DogadjajId { get; set; }
        public string NazivDogadjaja { get; set; }
        public Guid PredavacId { get; set; }
        public string ImePredavaca { get; set; }
        public string PrezimePredavaca { get; set; }
    }
}