using Microsoft.AspNetCore.Mvc;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.CreateDogadjaj;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.DeleteDogadjaj;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.UpdateDogadjaj;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Mediator;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Queries.GetAllDogadjaji;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Queries.GetDogadjajById;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Queries.SearchDogadjaji;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Controllers
{
    [ApiController]
    [Route("api/cqrs/dogadjaji")]
    public class DogadjajiCQRSController : ControllerBase
    {
        private readonly IMediator _mediator;

        private readonly UpdateDogadjajCommandHandler _updateHandler;

        private readonly DeleteDogadjajCommandHandler _deleteHandler;

        private readonly GetDogadjajByIdQueryHandler _getByIdHandler;

        private readonly SearchDogadjajiQueryHandler _searchHandler;

        public DogadjajiCQRSController(
            IMediator mediator,
            UpdateDogadjajCommandHandler updateHandler,
            DeleteDogadjajCommandHandler deleteHandler,
            GetDogadjajByIdQueryHandler getByIdHandler,
            SearchDogadjajiQueryHandler searchHandler)
        {
            _mediator = mediator;

            _updateHandler = updateHandler;

            _deleteHandler = deleteHandler;

            _getByIdHandler = getByIdHandler;

            _searchHandler = searchHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateDogadjajCommand command)
        {
            try
            {
                var id = await _mediator.SendAsync<Guid>(command);

                return Ok(new
                {
                    Message = "Događaj uspešno kreiran.",
                    DogadjajId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            UpdateDogadjajCommand command)
        {
            try
            {
                command.Id = id;

                await _updateHandler.HandleAsync(command);

                return Ok(new
                {
                    Message = "Događaj uspešno izmenjen."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var command = new DeleteDogadjajCommand
                {
                    Id = id
                };

                await _deleteHandler.HandleAsync(command);

                return Ok(new
                {
                    Message = "Događaj uspešno obrisan."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllDogadjajiQuery();

            var result = await _mediator
                .SendAsync<List<Dogadjaj>>(query);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetDogadjajByIdQuery
            {
                Id = id
            };

            var result = await _getByIdHandler
                .HandleAsync(query);

            if (result == null)
            {
                return NotFound(new
                {
                    Message = "Događaj nije pronađen."
                });
            }

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string naziv)
        {
            var query = new SearchDogadjajiQuery
            {
                Naziv = naziv
            };

            var result = await _searchHandler
                .HandleAsync(query);

            return Ok(result);
        }
    }
}