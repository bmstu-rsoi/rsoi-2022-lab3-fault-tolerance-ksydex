using System.Text;
using System.Text.Json;
using LoyaltyService.Data;
using LoyaltyService.Data.Dtos;
using LoyaltyService.Data.Entities;
using LoyaltyService.Helpers;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LoyaltyService.Services;

public class RabbitMQConsumer : BackgroundService
{
    private readonly ILogger<RabbitMQConsumer> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly AppDbContext _appDbContext;

    public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger, AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;

        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Port = 5672,
            UserName = "rabbitmq",
            Password = "rabbitmq",
            VirtualHost = "/"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "loyalty/update", exclusive: false, durable: true);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // Process the message
            _logger.LogInformation("Received message: {message}", message);

            // Acknowledge the message
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

            var dto = JsonSerializer.Deserialize<LoyaltyDto>(message);
            if (dto == null) return;
            
            var e = await _appDbContext.Set<Loyalty>()
                .SingleOrDefaultAsync(x => x.Id == dto.Id);
            
            if (e == null) return;
            
            e.UserName = dto.UserName;
            e.ReservationCount = dto.ReservationCount;

            e.Status = LoyaltyHelpers.CalcStatus(e);
            e.Discount = LoyaltyHelpers.CalcDiscount(e);

            await _appDbContext.SaveChangesAsync();
            _logger.LogInformation("Loyalty update succeed");
        };

        _channel.BasicConsume(queue: "loyalty/update",
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}