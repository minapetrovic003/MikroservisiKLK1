using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace OrganizacijaDogadjajaApp.APIGateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new()
                {
                    Title = "OrganizacijaDogadjaja API Gateway",
                    Version = "v1",
                    Description = "API Gateway za OrganizacijaDogadjajaApp mikroservise"
                });
            });

            builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddOcelot();

            builder.Services
                .AddOcelot(builder.Configuration)
                .AddCacheManager(x =>
                {
                    x.WithDictionaryHandle();
                });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(
                        "/swagger/v1/swagger.json",
                        "OrganizacijaDogadjaja API Gateway V1");
                });
            }

            app.UseHttpsRedirection();

            var pipeline = new OcelotPipelineConfiguration
            {
                AuthorizationMiddleware = async (context, next) =>
                {
                    var logger =
                        context.RequestServices.GetRequiredService<ILogger<Program>>();

                    logger.LogInformation(
                        "[GATEWAY] {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);

                    var clientId =
                        context.Request.Headers["X-Client-Id"]
                        .FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(clientId))
                    {
                        logger.LogWarning(
                            "[GATEWAY AUTH] Missing X-Client-Id header");
                    }
                    else
                    {
                        logger.LogInformation(
                            "[GATEWAY AUTH] ClientId: {ClientId}",
                            clientId);
                    }

                    await next.Invoke();
                }
            };

            await app.UseOcelot(pipeline);

            await app.RunAsync();
        }
    }
}