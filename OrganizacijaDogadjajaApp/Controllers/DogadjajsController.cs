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

        // RETRY + TIMEOUT
        public async Task<IActionResult> Index()
        {
            var dogadjajiClient = _httpClientFactory.CreateClient("DogadjajiAPI");

            try
            {
                var response = await dogadjajiClient.GetAsync("/Dogadjaji");

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                Console.WriteLine("JSON IZ API-ja:");
                Console.WriteLine(json);

                var dogadjaji = System.Text.Json.JsonSerializer.Deserialize<List<DogadjajDTO>>(
                    json,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                Console.WriteLine($"BROJ DOGADJAJA: {dogadjaji?.Count}");

                return View(dogadjaji ?? new List<DogadjajDTO>());
            }
            catch (TaskCanceledException ex) 
            { 
                Console.WriteLine("================================="); 
                Console.WriteLine("TIMEOUT DETEKTOVAN!");
                Console.WriteLine($"Vreme: {DateTime.Now}");
                Console.WriteLine($"Poruka: {ex.Message}");
                Console.WriteLine("DogadjajiAPI nije odgovorio na vreme."); 
                Console.WriteLine("================================="); 
                ViewBag.ExceptionMessage = "Timeout - DogadjajiAPI nije odgovorio na vreme.";
                return View(new List<DogadjajDTO>());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                ViewBag.ExceptionMessage = ex.Message;

                return View(new List<DogadjajDTO>());
            }
        }

        // CIRCUIT BREAKER
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