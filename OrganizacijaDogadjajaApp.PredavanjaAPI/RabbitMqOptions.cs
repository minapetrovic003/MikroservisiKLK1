namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Options
{
    //BITNO! -> Svi imaju isti Excheing, a ostalo mapitranje je zasebno 
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";

        // Exchange mora biti isti kao u DogadjajiAPI!
        public string Exchange { get; set; } = "dogadjaji.events";

        // Ovo je PREDAVANJA queue - UcesniciAPI ima svoju drugu queue
        public string Queue { get; set; } = "dogadjaji.events.predavanja";

        // Fanout ignorise routing key, ali ga treba deklarisati
        public string RoutingKey { get; set; } = "";

        // Koliko poruka odjednom uzimamo na obradu
        public ushort PrefetchCount { get; set; } = 1;
    }
}