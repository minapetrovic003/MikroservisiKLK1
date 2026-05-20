using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.UcesniciAPI.Data;
using OrganizacijaDogadjajaApp.UcesniciAPI.HostedServices;
using OrganizacijaDogadjajaApp.UcesniciAPI.Options;
using OrganizacijaDogadjajaApp.UcesniciAPI.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);



// ========================================
// RABBIT MQ OPTIONS
// ========================================

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));



// ========================================
// DB CONTEXT
// ========================================

builder.Services.AddDbContext<UcesniciDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));



// ========================================
// CONTROLLERS + SWAGGER
// ========================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();



// ========================================
// RABBIT MQ CONNECTION
// ========================================

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = "localhost"
    };

    return factory.CreateConnectionAsync().Result;
});



// ========================================
// SAGA PUBLISHER
// ========================================

builder.Services.AddScoped<ISagaPublisher, SagaPublisher>();



// ========================================
// POSTOJECI SERVISI
// ========================================

builder.Services.AddSingleton<DogadjajInfoClient>();

builder.Services.AddSingleton<IEmailQueuePublisher, EmailQueuePublisher>();



// ========================================
// HOSTED SERVICES
// ========================================

builder.Services.AddHostedService<RabbitMqConsumerHostedService>();

builder.Services.AddHostedService<EmailWorkerService>();



// ========================================
// SAGA COMPENSATION
// ========================================

builder.Services.AddHostedService<SagaCompensationHostedService>();



var app = builder.Build();



// ========================================
// SWAGGER
// ========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}



// ========================================
// MIDDLEWARE
// ========================================

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



// ========================================
// INITIALIZE CLIENT
// ========================================

var dogadjajClient =
    app.Services.GetRequiredService<DogadjajInfoClient>();

await dogadjajClient.InitializeAsync();



// ========================================
// START APP
// ========================================

app.Run();