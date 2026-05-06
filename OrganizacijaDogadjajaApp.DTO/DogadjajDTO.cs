namespace OrganizacijaDogadjajaApp.DTO
{
    //Na Mikroservisima sam radila preko klasa
    // u .Net sam radila kao record, nemam konkretno zahtev sto sam ovde odradila drugacije 
    //samo sam htela da vidim kako se radi sa klasama, ako mi ne odgovara vraticu se na rekord
   // rekord je inmutable a klasa ne 

    public class DogadjajDTO
    {
        public Guid Id { get; set; }
        public string NazivDogadjaja { get; set; }
        public string AgendaDogadjaja { get; set; }
        public DateTime DatumIVreme { get; set; }
        public int Trajanje { get; set; }
        public decimal CenaKotizacije { get; set; }
        public Guid LokacijaId { get; set; }
        public string NazivLokacije { get; set; }
        public Guid TipDogadjajaId { get; set; }
        public string NazivTipaDogadjaja { get; set; }
    }
}