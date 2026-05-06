namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Models
{
    public class Predavanje
    {
        public Guid Id { get; set; }
        public string Tema { get; set; }
        public int TrajanjePredavanja { get; set; }
        public DateTime Pocetak { get; set; }
        public Guid DogadjajId { get; set; }
        public Guid PredavacId { get; set; }
        public Predavac Predavac { get; set; }
    }
}