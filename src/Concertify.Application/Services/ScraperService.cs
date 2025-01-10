using System.Text;

using AutoMapper;

using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

namespace Concertify.Application.Services;

public class ScraperService(IScraperManager scraperManager, IMapper mapper) : IScraperService
{
    private readonly IScraperManager _scraperManager = scraperManager;
    private readonly IMapper _mapper = mapper;

    public async IAsyncEnumerable<ConcertSummaryDto> Collect()
    {
        Console.OutputEncoding = Encoding.UTF8;
        string url = "https://www.honarticket.com/#concerts-tehran";

        //List<ConcertSummaryDto> concerts = [];
        await foreach (var concert in _scraperManager.StartScraping(url))
        {
            yield return _mapper.Map<ConcertSummaryDto>(concert);
        }

    }
}
