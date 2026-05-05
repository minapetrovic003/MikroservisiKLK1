namespace OrganizacijaDogadjajaApp.Models
{
    public class Predavac
    {
        public Guid Id { get; set; }

        public string Ime { get; set; }

        public string Prezime { get; set; }

        public string Titula { get; set; }

        public string OblastStrucnosti { get; set; }

        public List<Predavanje> Predavanja { get; set; }
    }
}
