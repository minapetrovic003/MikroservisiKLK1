namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Entities
{
    //Outbox pattern
    //Problem -> dok se salje poruka veza pukne, podaci su u bazi ali ih ne vise
    //Resenje:
    //1. Save Event u DB
    //2.Save OutboxMessage u DB
    //3.BackgroundService kasnije publishuje
    public class OutboxMessage
    {
        public long Id { get; set; }

        public string EventType { get; set; }

        public string Payload { get; set; }

        // Kada je poruka kreirana - koristimo za sortiranje (starije prve)
        public DateTime CreatedAt { get; set; }
    }
}