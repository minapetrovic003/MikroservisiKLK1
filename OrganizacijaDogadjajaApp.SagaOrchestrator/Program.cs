using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Clients;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Data;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Options;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Services;

var builder = WebApplication.CreateBuilder(args);

// Učitaj opcije iz appsettings.json
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.Configure<ServiceUrlsOptions>(
    builder.Configuration.GetSection(ServiceUrlsOptions.SectionName));

// Baza podataka za Saga stanje
builder.Services.AddDbContext<SagaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registracija HTTP klijenata za pozivanje servisa
// AddHttpClient automatski kreira i reciklira HttpClient instance (efikasno!)
builder.Services.AddHttpClient<DogadjajiSagaClient>();
builder.Services.AddHttpClient<PredavanjaSagaClient>();
builder.Services.AddHttpClient<UcesniciSagaClient>();

// Registracija Saga Orchestrator servisa
// AddScoped = novi objekat za svaki HTTP zahtev (ispravno za DbContext)
builder.Services.AddScoped<PrijavaOrkestratorService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Saga Orchestrator API", Version = "v1" });
});

var app = builder.Build();

// Automatska migracija baze pri startu (samo za development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SagaDbContext>();
    await db.Database.MigrateAsync();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();