namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.CreateDogadjaj
{
    public class CreateDogadjajCommand
    {
        public string NazivDogadjaja { get; set; } = string.Empty;

        public string AgendaDogadjaja { get; set; } = string.Empty;

        public DateTime DatumIVreme { get; set; }

        public int Trajanje { get; set; }

        public decimal CenaKotizacije { get; set; }

        public Guid LokacijaId { get; set; }

        public Guid TipDogadjajaId { get; set; }
    }
}