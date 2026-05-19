namespace OrganizacijaDogadjajaApp.SagaOrchestrator.Entities
{
    /// <summary>
    /// Predstavlja jedan pokrenuti Saga proces u bazi podataka.
    /// Svaki put kad neko pokrene "prijavu na dogadjaj", kreira se jedan SagaInstance red.
    /// </summary>
    public class SagaInstance
    {
        public Guid Id { get; set; }

        // Koji korak je trenutno aktivan (1, 2, 3...)
        public int CurrentStep { get; set; }

        // Ukupan status Sage: Running, Completed, Failed, Compensating, Compensated
        public string Status { get; set; } = SagaStatus.Running;

        // Ulazni podaci – ID dogadjaja i ID ucesnika koji se prijavljuje
        public Guid DogadjajId { get; set; }
        public Guid UcesnikId { get; set; }

        // Rezultati koraka (cuvamo da bismo mogli da kompenzujemo)
        public Guid? RezervacijaId { get; set; }    // iz Koraka 1 (DogadjajiAPI)
        public Guid? RasporedId { get; set; }        // iz Koraka 2 (PredavanjaAPI)
        public Guid? PrijavaId { get; set; }         // iz Koraka 3 (UcesniciAPI)

        // Logovanje grešaka
        public string? GreskaOpis { get; set; }

        public DateTime KreiranaU { get; set; } = DateTime.UtcNow;
        public DateTime? AzuriranjaU { get; set; }
    }

    /// <summary>
    /// Konstante za status Sage – koristimo string umesto enum
    /// jer je lakše čitati u bazi i logovima.
    /// </summary>
    public static class SagaStatus
    {
        public const string Running = "Running";           // Saga teče normalno
        public const string Completed = "Completed";       // Sve uspešno završeno
        public const string Failed = "Failed";             // Pao je korak, pokrećemo kompenzacije
        public const string Compensating = "Compensating"; // Kompenzacije su u toku
        public const string Compensated = "Compensated";  // Sve kompenzacije završene
    }
}