using System.Text;

using AutoMapper;

using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

namespace Concertify.Application.Services;

public class ScraperService(IScraperManager scraperManager, IGenericRepository<Concert> genericRepository, IMapper mapper) : IScraperService
{
    private readonly IScraperManager _scraperManager = scraperManager;
    private readonly IMapper _mapper = mapper;

    public async Task<List<ConcertSummaryDto>> Collect()
    {
        Console.OutputEncoding = Encoding.UTF8;

        string url = "https://www.honarticket.com/#concerts-tehran";
        List<Concert> scrapedConcerts = await _scraperManager.StartScraping(url);

        List<ConcertSummaryDto> concerts = _mapper.Map<List<ConcertSummaryDto>>(scrapedConcerts);

        return concerts;
    }
}
