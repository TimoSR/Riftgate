using API.Features.AuctionOperations.Domain.Entities;
using API.Features.AuctionOperations.Domain.Events;
using API.Features.AuctionOperations.Domain.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Features.AuctionOperations.Domain;

public class BuyoutAuction : Auction
{
    [BsonElement("buyoutAmount")]
    public Price BuyoutAmount { get; }
    
    public BuyoutAuction(
        string sellerId, 
        Item item, 
        AuctionLength auctionLength, 
        Price buyout) 
        : base(sellerId, item, auctionLength)
    {
        BuyoutAmount = buyout ?? throw new ArgumentNullException(nameof(buyout));
    }
    
    // Public (Input Should be Validated)

    public override void PlaceBid(Bid bid)
    {
        ValidateBid(bid);
        
        Bids.Add(bid);
        
        AddDomainEvent(new BidPlacedEvent(Id, bid));

        if (bid.BidAmount.Value == BuyoutAmount.Value)
        {
            CompleteAuction(bid.TimeStamp);
        }
    }

    
    // Private

    protected override void ValidateBid(Bid bid)
    {
        if (bid == null)
            throw new ArgumentNullException(nameof(bid), "Bid cannot be null.");
        
        if (!IsActive)
            throw new InvalidOperationException("Attempted to place a bid on an inactive auction.");
        
        var highestBid = GetCurrentHighestBid();

        if (highestBid != null && bid.BidAmount.Value <= highestBid.BidAmount.Value)
        {
            throw new InvalidOperationException($"Bid amount of {bid.BidAmount.Value} must be higher than the current highest bid of {highestBid.BidAmount.Value}.");
        }

        if (bid.BidAmount.Value > BuyoutAmount.Value)
        {
            throw new InvalidOperationException($"Bid of {bid.BidAmount.Value} exceeds or equals the buyout price of {BuyoutAmount.Value}, which is not allowed.");
        }
    }
}