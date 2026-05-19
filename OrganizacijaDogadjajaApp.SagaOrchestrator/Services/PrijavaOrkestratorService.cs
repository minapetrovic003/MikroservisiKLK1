using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Clients;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Data;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Entities;

namespace OrganizacijaDogadjajaApp.SagaOrchestrator.Services
{
    /// <summary>
    /// Saga Orchestrator za poslovni proces "Prijava na dogadjaj".
    /// 
    /// Tok:
    ///   Korak 1: DogadjajiAPI  -> Rezerviši mesto
    ///   Korak 2: PredavanjaAPI -> Kreiraj raspored predavanja
    ///   Korak 3: UcesniciAPI   -> Potvrdi prijavu i pošalji email
    /// 
    /// Kompenzacije (unazad ako nešto padne):
    ///   Korak 3 padne -> ništa da se kompenzuje (nije ništa kreirano)
    ///   Korak 2 padne -> kompenzuj Korak 1: otkaži rezervaciju
    ///   Korak 3 padne -> kompenzuj Korak 2: obriši raspored
    ///                 -> kompenzuj Korak 1: otkaži rezervaciju
    /// </summary>
    public class PrijavaOrkestratorService
    {
        private readonly SagaDbContext _db;
        private readonly DogadjajiSagaClient _dogadjajiClient;
        private readonly PredavanjaSagaClient _predavanjaClient;
        private readonly UcesniciSagaClient _ucesniciClient;
        private readonly ILogger<PrijavaOrkestratorService> _logger;

        public PrijavaOrkestratorService(
            SagaDbContext db,
            DogadjajiSagaClient dogadjajiClient,
            PredavanjaSagaClient predavanjaClient,
            UcesniciSagaClient ucesniciClient,
            ILogger<PrijavaOrkestratorService> logger)
        {
            _db = db;
            _dogadjajiClient = dogadjajiClient;
            _predavanjaClient = predavanjaClient;
            _ucesniciClient = ucesniciClient;
            _logger = logger;
        }

        /// <summary>
        /// Pokretanje Sage. Kreira SagaInstance i prolazi kroz sve korake.
        /// Vraća ID kreirane Saga instance.
        /// </summary>
        public async Task<Guid> PokrniPrijavaAsync(Guid dogadjajId, Guid ucesnikId)
        {
            // 1. Kreiramo Saga instancu u bazi – odmah, pre bilo čega
            var saga = new SagaInstance
            {
                Id = Guid.NewGuid(),
                DogadjajId = dogadjajId,
                UcesnikId = ucesnikId,
                CurrentStep = 0,
                Status = SagaStatus.Running,
                KreiranaU = DateTime.UtcNow
            };

            _db.SagaInstances.Add(saga);
            await _db.SaveChangesAsync();

            _logger.LogInformation("[SAGA START] SagaId={SagaId} | DogadjajId={DogadjajId} | UcesnikId={UcesnikId}",
                saga.Id, dogadjajId, ucesnikId);

            try
            {
                // ============================================================
                // KORAK 1: Rezerviši mesto na dogadjaju (DogadjajiAPI)
                // ============================================================
                await AzurirajKorakAsync(saga, 1);

                _logger.LogInformation("[SAGA KORAK 1] Rezervišem mesto. SagaId={SagaId}", saga.Id);

                var rezervacijaId = await _dogadjajiClient.RezervisiMestoAsync(dogadjajId, ucesnikId);

                if (rezervacijaId is null)
                {
                    // Korak 1 pao – nema šta da kompenzujemo (još ništa nije kreirano)
                    _logger.LogError("[SAGA KORAK 1 PALO] Rezervacija nije uspela. SagaId={SagaId}", saga.Id);
                    await PostaviGreskaAsync(saga, "Korak 1 pao: DogadjajiAPI nije mogao da rezerviše mesto.");
                    return saga.Id;
                }

                saga.RezervacijaId = rezervacijaId;
                await _db.SaveChangesAsync();
                _logger.LogInformation("[SAGA KORAK 1 OK] RezervacijaId={Id}. SagaId={SagaId}", rezervacijaId, saga.Id);

                // ============================================================
                // KORAK 2: Kreiraj raspored predavanja (PredavanjaAPI)
                // ============================================================
                await AzurirajKorakAsync(saga, 2);

                _logger.LogInformation("[SAGA KORAK 2] Kreiram raspored. SagaId={SagaId}", saga.Id);

                var rasporedId = await _predavanjaClient.KreirajRasporedAsync(dogadjajId, ucesnikId);

                if (rasporedId is null)
                {
                    _logger.LogError("[SAGA KORAK 2 PALO] Raspored nije kreiran. SagaId={SagaId}", saga.Id);
                    await PostaviGreskaAsync(saga, "Korak 2 pao: PredavanjaAPI nije mogao da kreira raspored.");

                    // KOMPENZACIJA: Otkaži rezervaciju iz Koraka 1
                    await IzvrsiKompenzacijeAsync(saga);
                    return saga.Id;
                }

                saga.RasporedId = rasporedId;
                await _db.SaveChangesAsync();
                _logger.LogInformation("[SAGA KORAK 2 OK] RasporedId={Id}. SagaId={SagaId}", rasporedId, saga.Id);

                // ============================================================
                // KORAK 3: Potvrdi prijavu (UcesniciAPI)
                // ============================================================
                await AzurirajKorakAsync(saga, 3);

                _logger.LogInformation("[SAGA KORAK 3] Potvrdjujem prijavu. SagaId={SagaId}", saga.Id);

                var prijavaId = await _ucesniciClient.PotvrdPrijavaAsync(dogadjajId, ucesnikId, rezervacijaId.Value);

                if (prijavaId is null)
                {
                    _logger.LogError("[SAGA KORAK 3 PALO] Prijava nije potvrđena. SagaId={SagaId}", saga.Id);
                    await PostaviGreskaAsync(saga, "Korak 3 pao: UcesniciAPI nije mogao da potvrdi prijavu.");

                    // KOMPENZACIJA: Obriši raspored (Korak 2) i rezervaciju (Korak 1)
                    await IzvrsiKompenzacijeAsync(saga);
                    return saga.Id;
                }

                saga.PrijavaId = prijavaId;

                // ============================================================
                // SVE USPEŠNO – Saga završena
                // ============================================================
                saga.Status = SagaStatus.Completed;
                saga.CurrentStep = 3;
                saga.AzuriranjaU = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "[SAGA COMPLETED] SagaId={SagaId} | RezervacijaId={R} | RasporedId={Rp} | PrijavaId={P}",
                    saga.Id, saga.RezervacijaId, saga.RasporedId, saga.PrijavaId);

                return saga.Id;
            }
            catch (Exception ex)
            {
                // Neočekivana greška – loguj i kompenziraj
                _logger.LogError(ex, "[SAGA EXCEPTION] Neočekivana greška u Sagi. SagaId={SagaId}", saga.Id);
                await PostaviGreskaAsync(saga, $"Neočekivana greška: {ex.Message}");
                await IzvrsiKompenzacijeAsync(saga);
                return saga.Id;
            }
        }

        /// <summary>
        /// Izvršava kompenzacione akcije UNAZAD – od poslednjeg uspešnog koraka.
        /// 
        /// Primer: ako je Korak 3 pao, a Koraci 1 i 2 su uspeli:
        ///   - Kompenzuj Korak 2: obriši raspored
        ///   - Kompenzuj Korak 1: otkaži rezervaciju
        /// 
        /// VAŽNO: Kompenzacije idu unazad (od većeg ka manjem broju koraka)!
        /// </summary>
        private async Task IzvrsiKompenzacijeAsync(SagaInstance saga)
        {
            saga.Status = SagaStatus.Compensating;
            await _db.SaveChangesAsync();

            _logger.LogWarning("[SAGA KOMPENZACIJA POCETAK] SagaId={SagaId} | CurrentStep={Step}",
                saga.Id, saga.CurrentStep);

            // Kompenzacija Koraka 2: brisanje rasporeda (ako postoji)
            if (saga.RasporedId.HasValue)
            {
                _logger.LogWarning("[SAGA KOMPENZACIJA] Brišem raspored {Id}. SagaId={SagaId}",
                    saga.RasporedId, saga.Id);

                var uspelo = await _predavanjaClient.ObrisiRasporedAsync(saga.RasporedId.Value);

                if (!uspelo)
                    _logger.LogError("[SAGA KOMPENZACIJA GREŠKA] Nije uspelo brisanje rasporeda {Id}!", saga.RasporedId);
            }

            // Kompenzacija Koraka 1: otkazivanje rezervacije (ako postoji)
            if (saga.RezervacijaId.HasValue)
            {
                _logger.LogWarning("[SAGA KOMPENZACIJA] Otkazujem rezervaciju {Id}. SagaId={SagaId}",
                    saga.RezervacijaId, saga.Id);

                var uspelo = await _dogadjajiClient.OtkaziRezervacijuAsync(saga.RezervacijaId.Value);

                if (!uspelo)
                    _logger.LogError("[SAGA KOMPENZACIJA GREŠKA] Nije uspelo otkazivanje rezervacije {Id}!", saga.RezervacijaId);
            }

            saga.Status = SagaStatus.Compensated;
            saga.AzuriranjaU = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogWarning("[SAGA KOMPENZACIJA ZAVRSENA] SagaId={SagaId}", saga.Id);
        }

        /// <summary>
        /// Helper: ažurira koji je korak aktivan i snima u bazu.
        /// </summary>
        private async Task AzurirajKorakAsync(SagaInstance saga, int korak)
        {
            saga.CurrentStep = korak;
            saga.AzuriranjaU = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Helper: postavlja Sagu u Failed stanje sa opisom greške.
        /// </summary>
        private async Task PostaviGreskaAsync(SagaInstance saga, string opis)
        {
            saga.Status = SagaStatus.Failed;
            saga.GreskaOpis = opis;
            saga.AzuriranjaU = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}