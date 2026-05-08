namespace OrganizacijaDogadjajaApp.UcesniciAPI.Options
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Exchange { get; set; } = "dogadjaji.events";

        
        public string Queue { get; set; } = "dogadjaji.events.ucesnici";
        public string RoutingKey { get; set; } = "";
        public ushort PrefetchCount { get; set; } = 1;

        public string DogadjajInfoRequestQueue { get; set; } = "dogadjaji.info.request";
        public string DogadjajInfoReplyQueue { get; set; } = "dogadjaji.info.reply.ucesnici";
    }
}