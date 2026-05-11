using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.Data;
using OrganizacijaDogadjajaApp.Patterns;

namespace OrganizacijaDogadjajaApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //"registrovan" CircuitBreaker 
            builder.Services.AddSingleton<CircuitBreaker>(sp =>
                new CircuitBreaker(3, TimeSpan.FromSeconds(10))
            );

            // TIMEOUT + HTTPS sertifikat za DogadjajiAPI
            builder.Services.AddHttpClient("DogadjajiAPI", (client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.BaseAddress = new Uri(
                    builder.Configuration.GetValue<string>("DogadjajiAPIEndpoint")!);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });

            // PredavanjaAPI
            builder.Services.AddHttpClient("PredavanjaAPI", (client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.BaseAddress = new Uri(
                    builder.Configuration.GetValue<string>("PredavanjaAPIEndpoint")!);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });

            // UcesniciAPI
            builder.Services.AddHttpClient("UcesniciAPI", (client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.BaseAddress = new Uri(
                    builder.Configuration.GetValue<string>("UcesniciAPIEndpoint")!);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}