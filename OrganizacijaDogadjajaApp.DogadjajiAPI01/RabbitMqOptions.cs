namespace OrganizacijaDogadjajaApp.DogadjajiAPI
{
    //Ovde mapiramo dogadjaj
    //mapp konfiguraciju appSetings.json
    public class RabbitMqOptions
    {
        // Ovo je naziv sekcije u appsettings.json
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";

        // Exchange je kao "posta" - svi consumeri se prijave na njega
        // Fanout znaci: posalji SVIM koji su se prijavili
        public string Exchange { get; set; } = "dogadjaji.events";

        //Za request-reply zahteve
        public string DogadjajInfoRequestQueue { get; set; } = "dogadjaji.info.request";
    }
}
