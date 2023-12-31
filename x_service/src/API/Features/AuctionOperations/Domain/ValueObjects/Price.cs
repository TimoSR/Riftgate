using CodingPatterns.DomainLayer;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Features.AuctionOperations.Domain.ValueObjects;

public record Price : IValueObject
{
    [BsonElement("value")]
    public decimal Value { get; }

    public Price(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("BidAmount cannot be negative or zero.");

        Value = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}
