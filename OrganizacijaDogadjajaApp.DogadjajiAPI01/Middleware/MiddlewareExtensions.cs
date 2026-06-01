namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseQueryProtection(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<QueryProtectionMiddleware>();
        }
    }
}