using CodingPatterns.DomainLayer;

namespace API.Features.AuctionOperations.Domain.Services;

public interface IAuctionExpiryChecker
{
    Task CheckAndCompleteExpiredAuctions();
}