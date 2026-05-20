namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Mediator
{
    public interface IMediator
    {
        Task<TResult> SendAsync<TResult>(object request);
    }
}