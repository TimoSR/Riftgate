using System.ComponentModel.DataAnnotations;
using API.Features.AuctionOperations.Domain.Entities;
using API.Features.AuctionOperations.Domain.Repositories;
using API.Features.AuctionOperations.Domain.Services;
using API.Features.AuctionOperations.Domain.ValueObjects;
using AutoMapper;
using CodingPatterns.ApplicationLayer.ApplicationServices;
using CodingPatterns.ApplicationLayer.ServiceResultPattern;
using Infrastructure.ValidationAttributes;

namespace API.Features.AuctionOperations.Application.CommandHandlers.PlaceBid;

public class PlaceBid : ICommandHandler<PlaceBidCommand>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly ILogger<PlaceBid> _logger;
    private readonly ITimeService _timeService;

    public PlaceBid(
        IAuctionRepository auctionRepository, 
        ILogger<PlaceBid> logger, 
        ITimeService timeService)
    {
        _auctionRepository = auctionRepository;
        _logger = logger;
        _timeService = timeService;
    }

    public async Task<ServiceResult> Handle(PlaceBidCommand command)
    {
        var auction = await _auctionRepository.GetByIdAsync(command.AuctionId);

        var price = new Price(command.BidAmount);
        var bid = new Bid(command.BidderId, price, _timeService);
        
        auction.PlaceBid(bid);

        await _auctionRepository.UpdateAsync(auction);

        _logger.LogInformation("Bid placed successfully for Auction ID {AuctionID}.", command.AuctionId);
        return ServiceResult.Success("Bid placed successfully.");
    }
}

// For Internal Concerns

public record struct PlaceBidCommand(
    Guid RequestId, 
    string AuctionId, 
    string BidderId,
    decimal BidAmount) : ICommand;

// Requests have the responsibility to fail fast and be the endpoint contract

public record struct PlaceBidRequest : IRequest
{
    [Required]
    public Guid RequestId { get; set; }
    
    [Required(ErrorMessage = "The {0} field is required", AllowEmptyStrings = false)]
    [HexString(24)]
    public string AuctionId { get; set; }
    
    [Required]
    [HexString(24)]
    public string BidderId { get; set; }
    
    [Required(ErrorMessage = "Buyout amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Buyout amount must be greater than 0.")]
    public decimal BidAmount { get; set; }
}

public class PlaceBidProfile : Profile
{
    public PlaceBidProfile()
    {
        CreateMap<PlaceBidRequest, PlaceBidCommand>();
    }
}