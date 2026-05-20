namespace OrganizacijaDogadjajaApp.DTO.EventSaga
{
    //Uspesno rezervisano -> Salji ovaj event
    public class PredavanjeRezervisanoEvent
    {
        public Guid SagaId { get; set; }

        public Guid PrijavaId { get; set; }

        public Guid PredavanjeId { get; set; }

        public DateTime ReservedAt { get; set; }
    }
}