using System.Linq.Expressions;

using AutoMapper;

using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Exceptions;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

namespace Concertify.Application.Services;

public class ConcertService : IConcertService
{
    private readonly IGenericRepository<Concert> _concertRepository;
    private readonly IMapper _mapper;

    public ConcertService(IGenericRepository<Concert> concertRepository, IMapper mapper)
    {
        _concertRepository = concertRepository;
        _mapper = mapper;
    }

    public async Task<ConcertDetailsDto> GetConcertByIdAsync(int concertId)
    {
        Concert entity = await _concertRepository.GetByIdAsync(concertId)
            ?? throw new ItemNotFoundException(concertId);

        ConcertDetailsDto concert = _mapper.Map<ConcertDetailsDto>(entity);

        return concert;

    }

    public async Task<List<ConcertSummaryDto>> GetConcertsAsync(ConcertFilterDto concertFilterDto)
    {
        Expression<Func<Concert, bool>>[] filters =
        [
            c => concertFilterDto.Title == null || c.Title.StartsWith(concertFilterDto.Title),
            c => concertFilterDto.StartRange == null || c.StartDate.Contains(concertFilterDto.StartRange),
            c => concertFilterDto.EndRange == null || c.StartDate.Contains(concertFilterDto.EndRange),
            c => concertFilterDto.Province == null || (c.Province != null && c.Province.StartsWith(concertFilterDto.Province)),
            c => concertFilterDto.City == null || (c.City != null && c.City.StartsWith(concertFilterDto.City)),
            c => concertFilterDto.Category == null || c.Category.StartsWith(concertFilterDto.Category),
            c => concertFilterDto.TicketPriceRangeStart == null || c.TicketPrice.Contains(concertFilterDto.TicketPriceRangeStart),
            c => concertFilterDto.TicketPriceRangeEnd == null || c.TicketPrice.Contains(concertFilterDto.TicketPriceRangeEnd)
            //c => concertFilterDto.TicketPriceRangeStart == null || c.TicketPrice >= concertFilterDto.TicketPriceRangeStart,
            //c => concertFilterDto.TicketPriceRangeEnd== null || c.TicketPrice >= concertFilterDto.TicketPriceRangeEnd,
        ];

        List<Concert> concerts = await _concertRepository.GetFilteredAsync(filters, concertFilterDto.Skip, concertFilterDto.Take);

        List<ConcertSummaryDto> concertDtos = _mapper.Map<List<ConcertSummaryDto>>(concerts);

        return concertDtos;
    }
}
