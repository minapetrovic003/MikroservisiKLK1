namespace OrganizacijaDogadjajaApp.DTO.EventSaga
{   //Koreografija
    //Uspesno rezervisano -> Salji ovaj event
    public class PredavanjeRezervisanoEvent
    {
        public Guid SagaId { get; set; }
        //SagaIdpomocu njega pratimo stanje Saga procesa

        public Guid PrijavaId { get; set; }

        public Guid PredavanjeId { get; set; }

        public DateTime ReservedAt { get; set; }
    }
}