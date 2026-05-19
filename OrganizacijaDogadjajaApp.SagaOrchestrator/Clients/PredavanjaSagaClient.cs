using Microsoft.Extensions.Options;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Options;
using System.Text;
using System.Text.Json;

namespace OrganizacijaDogadjajaApp.SagaOrchestrator.Clients
{
    public class PredavanjaSagaClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PredavanjaSagaClient> _logger;

        public PredavanjaSagaClient(HttpClient httpClient, IOptions<ServiceUrlsOptions> options,
            ILogger<PredavanjaSagaClient> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.PredavanjaApi);
            _logger = logger;
        }

        /// <summary>
        /// Korak 2 normalna akcija: kreiraj raspored predavanja za učesnika.
        /// </summary>
        public async Task<Guid?> KreirajRasporedAsync(Guid dogadjajId, Guid ucesnikId)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new { DogadjajId = dogadjajId, UcesnikId = ucesnikId });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                _logger.LogInformation("[SAGA] Pozivam PredavanjaAPI - KreirajRaspored. DogadjajId={DogadjajId}", dogadjajId);

                var response = await _httpClient.PostAsync("/SagaRasporedi", content);

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var rasporedId = JsonSerializer.Deserialize<Guid>(body);
                    _logger.LogInformation("[SAGA] PredavanjaAPI - Raspored kreiran. RasporedId={Id}", rasporedId);
                    return rasporedId;
                }

                _logger.LogWarning("[SAGA] PredavanjaAPI - Raspored nije kreiran. StatusCode={Code}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SAGA] PredavanjaAPI - Greška pri kreiranju rasporeda.");
                return null;
            }
        }

        /// <summary>
        /// Kompenzaciona akcija za Korak 2: obriši raspored.
        /// </summary>
        public async Task<bool> ObrisiRasporedAsync(Guid rasporedId)
        {
            try
            {
                _logger.LogInformation("[SAGA KOMPENZACIJA] Brišem raspored {Id}", rasporedId);

                var response = await _httpClient.DeleteAsync($"/SagaRasporedi/{rasporedId}");

                _logger.LogInformation("[SAGA KOMPENZACIJA] Raspored {Id} obrisan.", rasporedId);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SAGA KOMPENZACIJA] Greška pri brisanju rasporeda {Id}.", rasporedId);
                return false;
            }
        }
    }
}