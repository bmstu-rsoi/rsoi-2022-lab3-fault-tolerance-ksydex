using LoyaltyService.Data;
using LoyaltyService.Data.Entities;
using LoyaltyService.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel.MessageTypes;

namespace LoyaltyService.Services.MessageConsumers;

public class LoyaltyUpdateMessageConsumer : IConsumer<LoyaltyUpdateMessage>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<LoyaltyUpdateMessageConsumer> _logger;

    public LoyaltyUpdateMessageConsumer(AppDbContext dbContext, ILogger<LoyaltyUpdateMessageConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LoyaltyUpdateMessage> context)
    {
        var message = context.Message;
        var e = await _dbContext.Set<Loyalty>()
            .SingleOrDefaultAsync(x => x.UserName == message.UserName);

        if (e == null)
        {
            _logger.LogInformation("Loyalty update failed for user" + message.UserName);
            return;
        }

        e.ReservationCount += message.CountDelta;

        e.Status = LoyaltyHelpers.CalcStatus(e);
        e.Discount = LoyaltyHelpers.CalcDiscount(e);

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Loyalty update succeed: " + message.CountDelta);
    }
}