namespace OrganizacijaDogadjajaApp.SagaOrchestrator.Options
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";

        // Queue na koju orkestrator šalje naredbe
        public string SagaCommandQueue { get; set; } = "saga.commands";
    }
}