namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Models
{
    /// <summary>
    /// Raspored predavanja za učesnika – kreiran od strane Saga Orchestratora.
    /// </summary>
    public class SagaRaspored
    {
        public Guid Id { get; set; }
        public Guid DogadjajId { get; set; }
        public Guid UcesnikId { get; set; }
        public DateTime KreiranaU { get; set; }
        public bool Obrisan { get; set; }
    }
}