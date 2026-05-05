namespace OrganizacijaDogadjajaApp.Models
{
    public class Prijava
    {
        public Guid Id { get; set; }

        public DateTime DatumPrijave { get; set; }

      
        public Guid DogadjajId { get; set; }
        public Dogadjaj Dogadjaj { get; set; }

        public Guid UcesnikId { get; set; }
        public Ucesnik Ucesnik { get; set; }
    }
}
