using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Middleware
{
    public class QueryProtectionMiddleware
    {
        private readonly RequestDelegate _next;

        public QueryProtectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            DogadjajiDbContext dbContext)
        {
            // Zanima nas samo GET (Query)
            if (context.Request.Method == HttpMethods.Get)
            {
                await _next(context);

                if (dbContext.ChangeTracker.HasChanges())
                {
                    throw new InvalidOperationException(
                        "Query operacija ne sme menjati stanje sistema.");
                }

                return;
            }

            await _next(context);
        }
    }
}