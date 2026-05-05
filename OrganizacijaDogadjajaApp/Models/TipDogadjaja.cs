namespace OrganizacijaDogadjajaApp.Models
{
    public class TipDogadjaja
    {
        public Guid Id { get; set; }

        public string Naziv { get; set; }

        public List<Dogadjaj> Dogadjaji { get; set; }
    }
}
