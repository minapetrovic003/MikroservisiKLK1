namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Entities
{
    //oUTbOX PATTERN
    public class OutboxMessage
    {
        public long Id { get; set; }

        // TIP DOGADJANA
        public string EventType { get; set; }

        // JSON string OVDE SU NAM PODACI 
        public string Payload { get; set; }

        // Kada je poruka kreirana - koristimo za sortiranje (starije prve)
        public DateTime CreatedAt { get; set; }
    }
}