namespace OrganizacijaDogadjajaApp.SagaOrchestrator.Options
{
    
    public class ServiceUrlsOptions
    {
        public const string SectionName = "ServiceUrls";

        public string DogadjajiApi { get; set; } = "https://localhost:7001";
        public string PredavanjaApi { get; set; } = "https://localhost:7002";
        public string UcesniciApi { get; set; } = "https://localhost:7003";
    }
}