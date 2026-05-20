namespace OrganizacijaDogadjajaApp.DTO.EventSaga
{
    //UcesnicApi -> Kreirana je prijava
    public class PrijavaKreiranaEvent
    {
        //Povezuje celu transakciju nadalje -> prostire se kroz app svi eventi znaju kojoj sagi pripadaju
        public Guid SagaId { get; set; }

        public Guid PrijavaId { get; set; }

        public Guid UcesnikId { get; set; }

        public Guid DogadjajId { get; set; }

        public Guid PredavanjeId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}