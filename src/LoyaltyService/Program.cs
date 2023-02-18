using LoyaltyService.Data;
using LoyaltyService.Data.MappingProfiles;
using LoyaltyService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration; // allows both to access and to set up the config
var environment = builder.Environment;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(x => { x.AddProfile<DefaultMappingProfile>(); });

builder.Services.AddDbContext<AppDbContext>(x =>
{
    x.UseSnakeCaseNamingConvention();
    x.UseNpgsql(
        // (environment.IsProduction()
        // ? GetProductionDbConnectionString(null)
        // : 
        configuration.GetConnectionString("DefaultConnection")
        // ) ??
        // throw new NullReferenceException("Database URL is not set!")
    );
});

// builder.Services.AddHostedService<RabbitMQConsumer>();

var app = builder.Build();

app.UseHttpLogging();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
catch (Exception e)
{
    // ignored
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(e, "An error occurred seeding the DB");
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();