using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DogadjajiAPI;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.CreateDogadjaj;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.DeleteDogadjaj;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.UpdateDogadjaj;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;
using OrganizacijaDogadjajaApp.DogadjajiAPI.HostedServices;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Mediator;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Queries.GetAllDogadjaji;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Queries.GetDogadjajById;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Queries.SearchDogadjaji;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Services;
using OrganizacijaDogadjajaApp.DogadjajiAPI01.HostedServices;
using OrganizacijaDogadjajaApp.DogadjajiAPI01.Services;
using RabbitMQ.Client;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);


// RabbitMQ options

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));


// DbContext

builder.Services.AddDbContext<DogadjajiDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// CQRS repositories

builder.Services.AddScoped<IDogadjajReadRepository, DogadjajReadRepository>();

builder.Services.AddScoped<IDogadjajWriteRepository, DogadjajWriteRepository>();


// CQRS handlers

builder.Services.AddScoped<CreateDogadjajCommandHandler>();

builder.Services.AddScoped<UpdateDogadjajCommandHandler>();

builder.Services.AddScoped<DeleteDogadjajCommandHandler>();

builder.Services.AddScoped<GetAllDogadjajiQueryHandler>();

builder.Services.AddScoped<GetDogadjajByIdQueryHandler>();

builder.Services.AddScoped<SearchDogadjajiQueryHandler>();


// Mediator pattern

builder.Services.AddScoped<IMediator, Mediator>();


// Controllers + Swagger

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


// RabbitMQ publisher

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();


// RabbitMQ connection

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = "localhost"
    };

    return factory.CreateConnectionAsync().Result;
});


// Hosted services

builder.Services.AddHostedService<OutboxMessagePublisher>();

builder.Services.AddHostedService<DogadjajInfoResponderService>();


// Saga services

builder.Services.AddScoped<ISagaPublisher, SagaPublisher>();

builder.Services.AddHostedService<SagaCompletionHostedService>();


var app = builder.Build();


// Swagger

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// Middleware

app.UseHttpsRedirection();

app.UseAuthorization();

//Middelwere
app.UseQueryProtection();

app.MapControllers();

app.Run();