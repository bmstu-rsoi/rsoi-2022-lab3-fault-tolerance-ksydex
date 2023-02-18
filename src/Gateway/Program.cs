using System.ComponentModel;
using Gateway.Helpers;
using Gateway.Middlewares;
using Gateway.Services;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration; // allows both to access and to set up the config
var environment = builder.Environment;
// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new DateTimeShortConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddScoped<RabbitMQProducer>();
// builder.Services.AddAutoMapper(x => { x.AddProfile<DefaultMappingProfile>(); });
//
// builder.Services.AddDbContext<AppDbContext>(x =>
// {
//     x.UseSnakeCaseNamingConvention();
//     x.UseNpgsql(
//         // (environment.IsProduction()
//         // ? GetProductionDbConnectionString(null)
//         // : 
//         configuration.GetConnectionString("DefaultConnection")
//         // ) ??
//         // throw new NullReferenceException("Database URL is not set!")
//     );
// });

builder.Services.AddOptions<RabbitMqTransportOptions>()
    .BindConfiguration(nameof(RabbitMqTransportOptions));

builder.Services.AddMassTransit(cfg =>
{
    cfg.ConfigureHealthCheckOptions(x =>
    {
        x.FailureStatus = HealthStatus.Degraded;
    });
    
    cfg.UsingRabbitMq((context, config) =>
    {
        config.UseJsonSerializer();
        config.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseHttpLogging();

Console.WriteLine($"ENV DEV: {app.Environment.IsDevelopment()}");

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

using var scope = app.Services.CreateScope();

app.UseMiddleware<ErrorHandlerMiddleware>();

var services = scope.ServiceProvider;
//
// try
// {
//     var context = services.GetRequiredService<AppDbContext>();
//     context.Database.Migrate();
// }
// catch (Exception e)
// {
//     // ignored
//     var logger = services.GetRequiredService<ILogger<Program>>();
//     logger.LogError(e, "An error occurred seeding the DB");
// }

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();