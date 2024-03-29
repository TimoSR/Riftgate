using CodingPatterns.InfrastructureLayer.IntegrationEvents;
using CodingPatterns.InfrastructureLayer.IntegrationEvents.GooglePubSub._Attributes;
using ProtoBuf;

namespace API.Features.AuctionOperations.Application.CommandHandlers.CreateBuyoutAuction.Topics;

[ProtoContract]
[TopicName("AuctionStartedTopic")]
public record AuctionStartedIntegrationEvent : IPublishEvent
{
    [ProtoMember(1)]
    public string AuctionId { get; init; }

    [ProtoMember(2)]
    public DateTime StartTime { get; init; }

    public AuctionStartedIntegrationEvent(string auctionId, DateTime startTime)
    {
        AuctionId = auctionId;
        StartTime = startTime;
    }

    public string Message => 
        $"Event: Auction {AuctionId} started at {StartTime:yyyy-MM-dd HH:mm:ss} (UTC).";
}