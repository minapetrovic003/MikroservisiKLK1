using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.Data;

namespace OrganizacijaDogadjajaApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Dodavanje servisa (MVC)
            builder.Services.AddControllersWithViews();

            // Dodavanje baze (DbContext)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Konfiguracija pipeline-a
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseStaticFiles(); // za css, js, slike

            app.UseAuthorization();

            // Ruta za kontrolere
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}