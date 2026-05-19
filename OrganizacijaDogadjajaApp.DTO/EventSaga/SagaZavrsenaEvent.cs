namespace OrganizacijaDogadjajaApp.DTO.Events
{
    //Final event -> Svi eventi uspesno zavrseni -> Saga gotova
    public class SagaZavrsenaEvent
    {
        public Guid SagaId { get; set; }

        public Guid PrijavaId { get; set; }

        public DateTime CompletedAt { get; set; }
    }
}