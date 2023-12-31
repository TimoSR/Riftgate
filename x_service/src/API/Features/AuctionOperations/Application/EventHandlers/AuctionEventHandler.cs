using API.Features.AuctionOperations.Domain.Events;
using MediatR;

namespace API.Features.AuctionOperations.Application.EventHandlers;

public class AuctionEventHandler : 
    INotificationHandler<AuctionStartedEvent>,
    INotificationHandler<AuctionCompletedEvent>,
    INotificationHandler<BidPlacedEvent>
{
    private ILogger<AuctionEventHandler> _logger;

    public AuctionEventHandler(ILogger<AuctionEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AuctionStartedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Handle(AuctionCompletedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Handle(BidPlacedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}