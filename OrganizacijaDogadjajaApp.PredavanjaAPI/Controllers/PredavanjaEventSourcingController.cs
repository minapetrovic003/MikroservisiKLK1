using Microsoft.AspNetCore.Mvc;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Aggregates;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Models;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Repositories;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Snapshots;

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Controllers
{
    [ApiController]
    [Route("api/event-sourcing/predavanja")]
    public class PredavanjaEventSourcingController : ControllerBase
    {
        private readonly IEventStoreRepository _repository;

        public PredavanjaEventSourcingController(
            IEventStoreRepository repository)
        {
            _repository = repository;
        }

        // CREATE

        [HttpPost]
        public async Task<IActionResult> Create(
            CreatePredavanjeRequest request)
        {
            var aggregateId = Guid.NewGuid();

            var aggregate = new PredavanjeAggregate(
                aggregateId,
                request.Naziv,
                request.Predavac,
                request.Sala,
                request.VremeOdrzavanja);

            await _repository.SaveEventsAsync(
                aggregateId,
                aggregate.GetUncommittedChanges());

            aggregate.MarkChangesAsCommitted();

            return Ok(new
            {
                AggregateId = aggregateId
            });
        }

        // CHANGE NAZIV

        [HttpPut("{id}/naziv")]
        public async Task<IActionResult> ChangeNaziv(
            Guid id,
            ChangeNazivRequest request)
        {
            var aggregate =
                await _repository.LoadAggregateAsync(id);

            if (aggregate == null)
                return NotFound();

            aggregate.ChangeNaziv(request.NoviNaziv);

            await _repository.SaveEventsAsync(
                id,
                aggregate.GetUncommittedChanges());

            aggregate.MarkChangesAsCommitted();

            return Ok();
        }

        // CHANGE SALA

        [HttpPut("{id}/sala")]
        public async Task<IActionResult> ChangeSala(
            Guid id,
            ChangeSalaRequest request)
        {
            var aggregate =
                await _repository.LoadAggregateAsync(id);

            if (aggregate == null)
                return NotFound();

            aggregate.ChangeSala(request.NovaSala);

            await _repository.SaveEventsAsync(
                id,
                aggregate.GetUncommittedChanges());

            aggregate.MarkChangesAsCommitted();

            return Ok();
        }

        // CHANGE PREDAVAC

        [HttpPut("{id}/predavac")]
        public async Task<IActionResult> ChangePredavac(
            Guid id,
            ChangePredavacRequest request)
        {
            var aggregate =
                await _repository.LoadAggregateAsync(id);

            if (aggregate == null)
                return NotFound();

            aggregate.ChangePredavac(request.NoviPredavac);

            await _repository.SaveEventsAsync(
                id,
                aggregate.GetUncommittedChanges());

            aggregate.MarkChangesAsCommitted();

            return Ok();
        }

        // CHANGE VREME

        [HttpPut("{id}/vreme")]
        public async Task<IActionResult> ChangeVreme(
            Guid id,
            ChangeVremeRequest request)
        {
            var aggregate =
                await _repository.LoadAggregateAsync(id);

            if (aggregate == null)
                return NotFound();

            aggregate.ChangeVreme(request.NovoVreme);

            await _repository.SaveEventsAsync(
                id,
                aggregate.GetUncommittedChanges());

            aggregate.MarkChangesAsCommitted();

            return Ok();
        }

        // CANCEL

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            CancelPredavanjeRequest request)
        {
            var aggregate =
                await _repository.LoadAggregateAsync(id);

            if (aggregate == null)
                return NotFound();

            aggregate.Cancel(request.Razlog);

            await _repository.SaveEventsAsync(
                id,
                aggregate.GetUncommittedChanges());

            aggregate.MarkChangesAsCommitted();

            return Ok();
        }

        // GET CURRENT STATE

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var aggregate =
                await _repository.LoadAggregateAsync(id);

            if (aggregate == null)
                return NotFound();

            return Ok(new
            {
                aggregate.Id,
                aggregate.Naziv,
                aggregate.Predavac,
                aggregate.Sala,
                aggregate.VremeOdrzavanja,
                aggregate.Otkazano,
                aggregate.Version
            });
        }

        // HISTORY

        [HttpGet("{id}/history")]
        public async Task<IActionResult> History(Guid id)
        {
            var events = await _repository.GetEventsAsync(id);

            return Ok(events);
        }

        // CREATE SNAPSHOT

        [HttpPost("{id}/snapshot")]
        public async Task<IActionResult> CreateSnapshot(Guid id)
        {
            var aggregate =
                await _repository.LoadAggregateAsync(id);

            if (aggregate == null)
                return NotFound();

            var snapshot = new PredavanjeSnapshot
            {
                Id = aggregate.Id,
                Naziv = aggregate.Naziv,
                Predavac = aggregate.Predavac,
                Sala = aggregate.Sala,
                VremeOdrzavanja = aggregate.VremeOdrzavanja,
                Otkazano = aggregate.Otkazano,
                Version = aggregate.Version
            };

            await _repository.SaveSnapshotAsync(
                id,
                snapshot,
                aggregate.Version);

            return Ok(new
            {
                Message = "Snapshot created."
            });
        }
    }
}