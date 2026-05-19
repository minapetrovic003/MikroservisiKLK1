namespace OrganizacijaDogadjajaApp.DTO.Events
{
    //Kompenzacioni event -> nije uspela prijava -> ucesniciApi brisu prijavu
    //Rollback
    public class PrijavaPonistenaEvent
    {
        public Guid SagaId { get; set; }

        public Guid PrijavaId { get; set; }

        public string Reason { get; set; } = string.Empty;

        public DateTime CancelledAt { get; set; }
    }
}