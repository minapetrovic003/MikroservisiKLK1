namespace OrganizacijaDogadjajaApp.Models
{
    public class Lokacija
    {
        public Guid Id { get; set; }

        public string Naziv { get; set; }

        public string Adresa { get; set; }

        public int Kapacitet { get; set; }

        public List<Dogadjaj> Dogadjaji { get; set; }
    }
}
