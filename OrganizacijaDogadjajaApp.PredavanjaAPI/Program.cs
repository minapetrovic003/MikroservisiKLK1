using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Data;
using OrganizacijaDogadjajaApp.PredavanjaAPI.HostedServices;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Options;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Services;
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

builder.Services.AddDbContext<PredavanjaDbContext>(options =>
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
// POSTOJECI CONSUMER
// ========================================

builder.Services.AddHostedService<RabbitMqConsumerHostedService>();



// ========================================
// SAGA SERVICES
// ========================================

builder.Services.AddScoped<ISagaPublisher, SagaPublisher>();

builder.Services.AddHostedService<SagaConsumerHostedService>();



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
// START APP
// ========================================

app.Run();