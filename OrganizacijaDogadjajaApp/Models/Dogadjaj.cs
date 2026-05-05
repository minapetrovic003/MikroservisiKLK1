namespace OrganizacijaDogadjajaApp.Models
{
   
        public class Dogadjaj
        {
            public Guid Id { get; set; }

            public string NazivDogadjaja { get; set; }

            public string AgendaDogadjaja { get; set; }

            public DateTime DatumIVreme { get; set; }

            public int Trajanje { get; set; }

            public decimal CenaKotizacije { get; set; }

            public Guid LokacijaId { get; set; }
            public Lokacija Lokacija { get; set; }

            public Guid TipDogadjajaId { get; set; }
            public TipDogadjaja TipDogadjaja { get; set; }
        }
   
}
