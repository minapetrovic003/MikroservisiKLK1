using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrganizacijaDogadjajaApp.DTO;
using OrganizacijaDogadjajaApp.Patterns;
using Polly;

namespace OrganizacijaDogadjajaApp.Controllers
{
    public class DogadjajsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CircuitBreaker _circuitBreaker;

        public DogadjajsController(IHttpClientFactory httpClientFactory, CircuitBreaker circuitBreaker)
        {
            _httpClientFactory = httpClientFactory;
            _circuitBreaker = circuitBreaker;
        }

        // GET: Dogadjajs — koristi RETRY + TIMEOUT
        public async Task<IActionResult> Index()
        {
            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            try
            {
                HttpResponseMessage? httpResponseMessage = null;

                var retryPolicy = Policy.Handle<HttpRequestException>()
                    .WaitAndRetryAsync(2, attempt => TimeSpan.FromMilliseconds(250));

                httpResponseMessage = await retryPolicy.ExecuteAsync(async () =>
                {
                    httpResponseMessage = await dogadjajiClient.GetAsync("/Dogadjaji");
                    httpResponseMessage.EnsureSuccessStatusCode();
                    return httpResponseMessage;
                });

                var dogadjaji = await httpResponseMessage.Content.ReadFromJsonAsync<List<DogadjajDTO>>();

                return View(dogadjaji);
            }
            catch (TaskCanceledException)
            {
                ViewBag.ExceptionMessage = "Servis za dogadjaje ne odgovara — timeout.";
                return View(new List<DogadjajDTO>());
            }
            catch (HttpRequestException)
            {
                ViewBag.ExceptionMessage = "Servis za dogadjaje nedostupan — iscrpljeni pokusaji.";
                return View(new List<DogadjajDTO>());
            }
        }

        // GET: Dogadjajs/Details/5 — koristi CIRCUIT BREAKER
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            try
            {
                var responseMessage = await _circuitBreaker.ExecuteAsync(async () =>
                {
                    var response = await dogadjajiClient.GetAsync($"/Dogadjaji/{id}");
                    response.EnsureSuccessStatusCode();
                    return response;
                });

                var dogadjaj = await responseMessage.Content.ReadFromJsonAsync<DogadjajDTO>();

                if (dogadjaj == null)
                    return NotFound();

                return View(dogadjaj);
            }
            catch (CircuitBreakerOpenException)
            {
                ViewBag.ExceptionMessage = "Servis privremeno nedostupan — circuit breaker aktivan.";
                return View(new DogadjajDTO());
            }
            catch (HttpRequestException)
            {
                ViewBag.ExceptionMessage = "Greska pri komunikaciji sa servisom za dogadjaje.";
                return View(new DogadjajDTO());
            }
        }

        // GET: Dogadjajs/Create
        public async Task<IActionResult> Create()
        {
            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            var lokacijeResponse = await dogadjajiClient.GetAsync("/Lokacije");
            var tipoviResponse = await dogadjajiClient.GetAsync("/TipoviDogadjaja");

            var lokacije = await lokacijeResponse.Content.ReadFromJsonAsync<List<LokacijaDTO>>();
            var tipovi = await tipoviResponse.Content.ReadFromJsonAsync<List<TipDogadjajaDTO>>();

            ViewData["LokacijaId"] = new SelectList(lokacije, "Id", "Naziv");
            ViewData["TipDogadjajaId"] = new SelectList(tipovi, "Id", "Naziv");

            return View();
        }

        // POST: Dogadjajs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NazivDogadjaja,AgendaDogadjaja,DatumIVreme,Trajanje,CenaKotizacije,LokacijaId,TipDogadjajaId")] DogadjajDTO dogadjajDTO)
        {
            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            var response = await dogadjajiClient.PostAsJsonAsync("/Dogadjaji", dogadjajDTO);
            response.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }

        // GET: Dogadjajs/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            var dogadjajResponse = await dogadjajiClient.GetAsync($"/Dogadjaji/{id}");
            var dogadjaj = await dogadjajResponse.Content.ReadFromJsonAsync<DogadjajDTO>();

            if (dogadjaj == null)
                return NotFound();

            var lokacijeResponse = await dogadjajiClient.GetAsync("/Lokacije");
            var tipoviResponse = await dogadjajiClient.GetAsync("/TipoviDogadjaja");
            var lokacije = await lokacijeResponse.Content.ReadFromJsonAsync<List<LokacijaDTO>>();
            var tipovi = await tipoviResponse.Content.ReadFromJsonAsync<List<TipDogadjajaDTO>>();

            ViewData["LokacijaId"] = new SelectList(lokacije, "Id", "Naziv", dogadjaj.LokacijaId);
            ViewData["TipDogadjajaId"] = new SelectList(tipovi, "Id", "Naziv", dogadjaj.TipDogadjajaId);

            return View(dogadjaj);
        }

        // POST: Dogadjajs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NazivDogadjaja,AgendaDogadjaja,DatumIVreme,Trajanje,CenaKotizacije,LokacijaId,TipDogadjajaId")] DogadjajDTO dogadjajDTO)
        {
            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            var response = await dogadjajiClient.PutAsJsonAsync($"/Dogadjaji/{id}", dogadjajDTO);
            response.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }

        // GET: Dogadjajs/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            var response = await dogadjajiClient.GetAsync($"/Dogadjaji/{id}");
            var dogadjaj = await response.Content.ReadFromJsonAsync<DogadjajDTO>();

            if (dogadjaj == null)
                return NotFound();

            return View(dogadjaj);
        }

        // POST: Dogadjajs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");
            await dogadjajiClient.DeleteAsync($"/Dogadjaji/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}