using Microsoft.Extensions.DependencyInjection;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.CreateDogadjaj;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Queries.GetAllDogadjaji;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Mediator
{
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResult> SendAsync<TResult>(object request)
        {
            switch (request)
            {
                case CreateDogadjajCommand command:

                    var createHandler = _serviceProvider
                        .GetRequiredService<CreateDogadjajCommandHandler>();

                    return (TResult)(object)
                        await createHandler.HandleAsync(command);

                case GetAllDogadjajiQuery query:

                    var getAllHandler = _serviceProvider
                        .GetRequiredService<GetAllDogadjajiQueryHandler>();

                    return (TResult)(object)
                        await getAllHandler.HandleAsync(query);

                default:
                    throw new Exception("Handler nije pronađen.");
            }
        }
    }
}