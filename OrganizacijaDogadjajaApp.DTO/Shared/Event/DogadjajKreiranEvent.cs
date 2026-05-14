namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Shared.Events
{
    public class DogadjajKreiranEvent
    {
        public Guid DogadjajId { get; set; }
        public string NazivDogadjaja { get; set; }
        public string AgendaDogadjaja { get; set; }
        public DateTime DatumIVreme { get; set; }
        public int Trajanje { get; set; }
        public string NazivLokacije { get; set; }
    }
}