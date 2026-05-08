using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.UcesniciAPI.Data;
using OrganizacijaDogadjajaApp.UcesniciAPI.HostedServices;
using OrganizacijaDogadjajaApp.UcesniciAPI.Options;
using OrganizacijaDogadjajaApp.UcesniciAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddDbContext<UcesniciDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DogadjajInfoClient>();

builder.Services.AddHostedService<RabbitMqConsumerHostedService>();

builder.Services.AddSingleton<IEmailQueuePublisher, EmailQueuePublisher>();
builder.Services.AddHostedService<EmailWorkerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var dogadjajClient =
    app.Services.GetRequiredService<DogadjajInfoClient>();

await dogadjajClient.InitializeAsync();

app.Run();