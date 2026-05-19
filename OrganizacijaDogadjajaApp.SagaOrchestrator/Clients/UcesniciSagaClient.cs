using Microsoft.Extensions.Options;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Options;
using System.Text;
using System.Text.Json;

namespace OrganizacijaDogadjajaApp.SagaOrchestrator.Clients
{
    public class UcesniciSagaClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UcesniciSagaClient> _logger;

        public UcesniciSagaClient(HttpClient httpClient, IOptions<ServiceUrlsOptions> options,
            ILogger<UcesniciSagaClient> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.UcesniciApi);
            _logger = logger;
        }

        /// <summary>
        /// Korak 3 normalna akcija: potvrdi prijavu učesnika.
        /// </summary>
        public async Task<Guid?> PotvrdPrijavaAsync(Guid dogadjajId, Guid ucesnikId, Guid rezervacijaId)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    DogadjajId = dogadjajId,
                    UcesnikId = ucesnikId,
                    RezervacijaId = rezervacijaId
                });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                _logger.LogInformation("[SAGA] Pozivam UcesniciAPI - PotvrdiPrijavu. UcesnikId={UcesnikId}", ucesnikId);

                var response = await _httpClient.PostAsync("/SagaPrijave", content);

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var prijavaId = JsonSerializer.Deserialize<Guid>(body);
                    _logger.LogInformation("[SAGA] UcesniciAPI - Prijava potvrđena. PrijavaId={Id}", prijavaId);
                    return prijavaId;
                }

                _logger.LogWarning("[SAGA] UcesniciAPI - Prijava nije potvrđena. StatusCode={Code}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SAGA] UcesniciAPI - Greška pri potvrdi prijave.");
                return null;
            }
        }

        /// <summary>
        /// Kompenzaciona akcija za Korak 3: otkaži prijavu.
        /// </summary>
        public async Task<bool> OtkaziPrijavaAsync(Guid prijavaId)
        {
            try
            {
                _logger.LogInformation("[SAGA KOMPENZACIJA] Otkazujem prijavu {Id}", prijavaId);

                var response = await _httpClient.DeleteAsync($"/SagaPrijave/{prijavaId}");

                _logger.LogInformation("[SAGA KOMPENZACIJA] Prijava {Id} otkazana.", prijavaId);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SAGA KOMPENZACIJA] Greška pri otkazivanju prijave {Id}.", prijavaId);
                return false;
            }
        }
    }
}