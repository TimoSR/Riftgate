using API.Features.AuctionListing.Application.DTO;
using API.Features.AuctionOperations.Domain.Repositories;
using AutoMapper;
using CodingPatterns.ApplicationLayer.ApplicationServices;
using CodingPatterns.ApplicationLayer.ServiceResultPattern;

namespace API.Features.AuctionOperations.Application.QueryHandlers;

public class GetAllActiveAuctions : IQueryHandler<GetAllActiveAuctionsQuery, ServiceResult<List<AuctionDto>>>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IMapper _mapper;

    public GetAllActiveAuctions(IAuctionRepository auctionRepository, IMapper mapper)
    {
        _auctionRepository = auctionRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResult<List<AuctionDto>>> Handle(GetAllActiveAuctionsQuery query)
    {
        try
        {
            var auctions = await _auctionRepository.GetAllActiveAuctionsAsync();
            var auctionDtos = _mapper.Map<List<AuctionDto>>(auctions);
            return ServiceResult<List<AuctionDto>>.Success(auctionDtos);
        }
        catch (Exception ex)
        {
            // Log the exception details and handle the error
            return ServiceResult<List<AuctionDto>>.Failure("Failed to retrieve active auctions.");
        }
    }
}

public class GetAllActiveAuctionsQuery : IQuery<ServiceResult<List<AuctionDto>>>
{
    // Currently no properties, but it's here to represent a specific querying intention
}
