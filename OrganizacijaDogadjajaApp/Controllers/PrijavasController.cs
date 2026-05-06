using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrganizacijaDogadjajaApp.DTO;
using OrganizacijaDogadjajaApp.Patterns;
using Polly;

namespace OrganizacijaDogadjajaApp.Controllers
{
    public class PrijavasController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CircuitBreaker _circuitBreaker;

        public PrijavasController(IHttpClientFactory httpClientFactory, CircuitBreaker circuitBreaker)
        {
            _httpClientFactory = httpClientFactory;
            _circuitBreaker = circuitBreaker;
        }

        // GET: Prijavas — RETRY + TIMEOUT
        public async Task<IActionResult> Index()
        {
            var ucesniciClient = _httpClientFactory.CreateClient("UcesniciAPI");

            try
            {
                HttpResponseMessage? httpResponseMessage = null;

                var retryPolicy = Policy.Handle<HttpRequestException>()
                    .WaitAndRetryAsync(2, attempt => TimeSpan.FromMilliseconds(250));

                httpResponseMessage = await retryPolicy.ExecuteAsync(async () =>
                {
                    httpResponseMessage = await ucesniciClient.GetAsync("/Prijave");
                    httpResponseMessage.EnsureSuccessStatusCode();
                    return httpResponseMessage;
                });

                var prijave = await httpResponseMessage.Content.ReadFromJsonAsync<List<PrijavaDTO>>();

                return View(prijave);
            }
            catch (TaskCanceledException)
            {
                ViewBag.ExceptionMessage = "Servis za ucesnike ne odgovara — timeout.";
                return View(new List<PrijavaDTO>());
            }
            catch (HttpRequestException)
            {
                ViewBag.ExceptionMessage = "Servis za ucesnike nedostupan — iscrpljeni pokusaji.";
                return View(new List<PrijavaDTO>());
            }
        }

        // GET: Prijavas/Create
        public async Task<IActionResult> Create()
        {
            var ucesniciClient = _httpClientFactory.CreateClient("UcesniciAPI");
            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            var ucesniciResponse = await ucesniciClient.GetAsync("/Ucesnici");
            var dogadjajiResponse = await dogadjajiClient.GetAsync("/Dogadjaji");

            var ucesnici = await ucesniciResponse.Content.ReadFromJsonAsync<List<UcesnikDTO>>();
            var dogadjaji = await dogadjajiResponse.Content.ReadFromJsonAsync<List<DogadjajDTO>>();

            ViewData["UcesnikId"] = new SelectList(ucesnici, "Id", "Email");
            ViewData["DogadjajId"] = new SelectList(dogadjaji, "Id", "NazivDogadjaja");

            return View();
        }

        // POST: Prijavas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DogadjajId,UcesnikId")] PrijavaDTO prijavaDTO)
        {
            var ucesniciClient = _httpClientFactory.CreateClient("UcesniciAPI");

            var response = await ucesniciClient.PostAsJsonAsync("/Prijave", prijavaDTO);
            response.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }

        // POST: Prijavas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ucesniciClient = _httpClientFactory.CreateClient("UcesniciAPI");
            await ucesniciClient.DeleteAsync($"/Prijave/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}