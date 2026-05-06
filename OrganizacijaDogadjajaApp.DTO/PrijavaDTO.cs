namespace OrganizacijaDogadjajaApp.DTO
{
    public class PrijavaDTO
    {
        public Guid Id { get; set; }
        public DateTime DatumPrijave { get; set; }
        public Guid DogadjajId { get; set; }
        public string NazivDogadjaja { get; set; }
        public Guid UcesnikId { get; set; }
        public string ImeUcesnika { get; set; }
        public string PrezimeUcesnika { get; set; }
    }
}