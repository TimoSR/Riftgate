namespace API.Features.AuctionOperations.Domain.Services;

public class TimeService : ITimeService
{
    public DateTime GetCurrentTime()
    {
        return DateTime.UtcNow;
    }
}