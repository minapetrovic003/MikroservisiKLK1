namespace OrganizacijaDogadjajaApp.DTO.EventSaga
{
    //Pokrece konpezaciju
    //Ako nema mesta na predavanju -> faliur event
    public class RezervacijaPredavanjaNeuspelaEvent
    {
        public Guid SagaId { get; set; }

        public Guid PrijavaId { get; set; }

        public Guid PredavanjeId { get; set; }

        public string Reason { get; set; } = string.Empty;
        //Zasto je doslo  do pucanja eventa

        public DateTime FailedAt { get; set; }
    }
}