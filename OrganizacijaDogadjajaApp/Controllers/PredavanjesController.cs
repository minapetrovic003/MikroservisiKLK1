using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrganizacijaDogadjajaApp.DTO;
using OrganizacijaDogadjajaApp.Patterns;
using Polly;

namespace OrganizacijaDogadjajaApp.Controllers
{
    public class PredavanjesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CircuitBreaker _circuitBreaker;

        public PredavanjesController(IHttpClientFactory httpClientFactory, CircuitBreaker circuitBreaker)
        {
            _httpClientFactory = httpClientFactory;
            _circuitBreaker = circuitBreaker;
        }

        // GET: Predavanjes — RETRY + TIMEOUT
        public async Task<IActionResult> Index()
        {
            var predavanjaClient = _httpClientFactory.CreateClient("PredavanjaAPI");

            try
            {
                HttpResponseMessage? httpResponseMessage = null;

                var retryPolicy = Policy.Handle<HttpRequestException>()
                    .WaitAndRetryAsync(2, attempt => TimeSpan.FromMilliseconds(250));

                httpResponseMessage = await retryPolicy.ExecuteAsync(async () =>
                {
                    httpResponseMessage = await predavanjaClient.GetAsync("/Predavanja");
                    httpResponseMessage.EnsureSuccessStatusCode();
                    return httpResponseMessage;
                });

                var predavanja = await httpResponseMessage.Content.ReadFromJsonAsync<List<PredavanjeDTO>>();

                return View(predavanja);
            }
            catch (TaskCanceledException)
            {
                ViewBag.ExceptionMessage = "Servis za predavanja ne odgovara — timeout.";
                return View(new List<PredavanjeDTO>());
            }
            catch (HttpRequestException)
            {
                ViewBag.ExceptionMessage = "Servis za predavanja nedostupan — iscrpljeni pokusaji.";
                return View(new List<PredavanjeDTO>());
            }
        }

        // GET: Predavanjes/Details/5 — CIRCUIT BREAKER
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var predavanjaClient = _httpClientFactory.CreateClient("PredavanjaAPI");

            try
            {
                var responseMessage = await _circuitBreaker.ExecuteAsync(async () =>
                {
                    var response = await predavanjaClient.GetAsync($"/Predavanja/{id}");
                    response.EnsureSuccessStatusCode();
                    return response;
                });

                var predavanje = await responseMessage.Content.ReadFromJsonAsync<PredavanjeDTO>();

                if (predavanje == null)
                    return NotFound();

                return View(predavanje);
            }
            catch (CircuitBreakerOpenException)
            {
                ViewBag.ExceptionMessage = "Servis privremeno nedostupan — circuit breaker aktivan.";
                return View(new PredavanjeDTO());
            }
            catch (HttpRequestException)
            {
                ViewBag.ExceptionMessage = "Greska pri komunikaciji sa servisom za predavanja.";
                return View(new PredavanjeDTO());
            }
        }

        // GET: Predavanjes/Create
        public async Task<IActionResult> Create()
        {
            var predavanjaClient = _httpClientFactory.CreateClient("PredavanjaAPI");
            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            var predavaciResponse = await predavanjaClient.GetAsync("/Predavaci");
            var dogadjajiResponse = await dogadjajiClient.GetAsync("/Dogadjaji");

            var predavaci = await predavaciResponse.Content.ReadFromJsonAsync<List<PredavacDTO>>();
            var dogadjaji = await dogadjajiResponse.Content.ReadFromJsonAsync<List<DogadjajDTO>>();

            ViewData["PredavacId"] = new SelectList(predavaci, "Id", "Ime");
            ViewData["DogadjajId"] = new SelectList(dogadjaji, "Id", "NazivDogadjaja");

            return View();
        }

        // POST: Predavanjes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tema,TrajanjePredavanja,Pocetak,DogadjajId,PredavacId")] PredavanjeDTO predavanjeDTO)
        {
            var predavanjaClient = _httpClientFactory.CreateClient("PredavanjaAPI");

            var response = await predavanjaClient.PostAsJsonAsync("/Predavanja", predavanjeDTO);
            response.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }

        // GET: Predavanjes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var predavanjaClient = _httpClientFactory.CreateClient("PredavanjaAPI");
            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            var predavanjeResponse = await predavanjaClient.GetAsync($"/Predavanja/{id}");
            var predavanje = await predavanjeResponse.Content.ReadFromJsonAsync<PredavanjeDTO>();

            if (predavanje == null)
                return NotFound();

            var predavaciResponse = await predavanjaClient.GetAsync("/Predavaci");
            var dogadjajiResponse = await dogadjajiClient.GetAsync("/Dogadjaji");
            var predavaci = await predavaciResponse.Content.ReadFromJsonAsync<List<PredavacDTO>>();
            var dogadjaji = await dogadjajiResponse.Content.ReadFromJsonAsync<List<DogadjajDTO>>();

            ViewData["PredavacId"] = new SelectList(predavaci, "Id", "Ime", predavanje.PredavacId);
            ViewData["DogadjajId"] = new SelectList(dogadjaji, "Id", "NazivDogadjaja", predavanje.DogadjajId);

            return View(predavanje);
        }

        // POST: Predavanjes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Tema,TrajanjePredavanja,Pocetak,DogadjajId,PredavacId")] PredavanjeDTO predavanjeDTO)
        {
            var predavanjaClient = _httpClientFactory.CreateClient("PredavanjaAPI");

            var response = await predavanjaClient.PutAsJsonAsync($"/Predavanja/{id}", predavanjeDTO);
            response.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }

        // GET: Predavanjes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var predavanjaClient = _httpClientFactory.CreateClient("PredavanjaAPI");

            var response = await predavanjaClient.GetAsync($"/Predavanja/{id}");
            var predavanje = await response.Content.ReadFromJsonAsync<PredavanjeDTO>();

            if (predavanje == null)
                return NotFound();

            return View(predavanje);
        }

        // POST: Predavanjes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var predavanjaClient = _httpClientFactory.CreateClient("PredavanjaAPI");
            await predavanjaClient.DeleteAsync($"/Predavanja/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}