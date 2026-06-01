using Microsoft.Extensions.Options;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Options;
using System.Text;
using System.Text.Json;

namespace OrganizacijaDogadjajaApp.SagaOrchestrator.Clients
{
    
    public class DogadjajiSagaClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DogadjajiSagaClient> _logger;

        public DogadjajiSagaClient(HttpClient httpClient, IOptions<ServiceUrlsOptions> options,
            ILogger<DogadjajiSagaClient> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.DogadjajiApi);
            _logger = logger;
        }

        /// <summary>
        /// Korak 1 normale akcije: rezerviši mesto na dogadjaju.
        /// Vraća ID rezervacije ili null ako nije uspelo.
        /// </summary>
        public async Task<Guid?> RezervisiMestoAsync(Guid dogadjajId, Guid ucesnikId)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new { DogadjajId = dogadjajId, UcesnikId = ucesnikId });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                _logger.LogInformation("[SAGA] Pozivam DogadjajiAPI - RezervisiMesto. DogadjajId={DogadjajId}", dogadjajId);

                var response = await _httpClient.PostAsync("/SagaRezervacije", content);

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var rezervacijaId = JsonSerializer.Deserialize<Guid>(body);
                    _logger.LogInformation("[SAGA] DogadjajiAPI - Rezervacija uspešna. RezervacijaId={Id}", rezervacijaId);
                    return rezervacijaId;
                }

                _logger.LogWarning("[SAGA] DogadjajiAPI - Rezervacija neuspešna. StatusCode={Code}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SAGA] DogadjajiAPI - Greška pri rezervisanju mesta.");
                return null;
            }
        }

        /// <summary>
        /// Kompenzaciona akcija za Korak 1: otkaži rezervaciju.
        /// Poziva se ako kasniji korak padne.
        /// </summary>
        public async Task<bool> OtkaziRezervacijuAsync(Guid rezervacijaId)
        {
            try
            {
                _logger.LogInformation("[SAGA KOMPENZACIJA] Otkazujem rezervaciju {Id}", rezervacijaId);

                var response = await _httpClient.DeleteAsync($"/SagaRezervacije/{rezervacijaId}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[SAGA KOMPENZACIJA] Rezervacija {Id} otkazana.", rezervacijaId);
                    return true;
                }

                _logger.LogWarning("[SAGA KOMPENZACIJA] Nije uspelo otkazivanje rezervacije {Id}. StatusCode={Code}",
                    rezervacijaId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SAGA KOMPENZACIJA] Greška pri otkazivanju rezervacije {Id}.", rezervacijaId);
                return false;
            }
        }
    }
}