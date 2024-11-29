using System.Text;

using AutoMapper;

using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

namespace Concertify.Application.Services;

public class ScraperService(IWebScraper scraper, IGenericRepository<Concert> genericRepository, IMapper mapper) : IScraperService
{
    private readonly IWebScraper _scraper = scraper;
    private readonly IGenericRepository<Concert> _concertRepository = genericRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<List<ConcertSummaryDto>> Collect()
    {
        Console.OutputEncoding = Encoding.UTF8;

        string url = "https://www.honarticket.com/#concerts-tehran";

        List<string> links = _scraper.ExtractLinks(url);

        List<Concert> concerts = [];

        foreach (string link in links)
        {
            Console.WriteLine("Scraping " + link);
            var concert = _scraper.Scrape(link);
            if (concert == null)
            {
                Console.WriteLine("there was a problem. the object was null, moving on to the next one.\n.");
                continue;
            }
            concerts.Add(concert);
            Console.WriteLine("Done!\n.");
        }

        Console.WriteLine("Done scraping.");

        concerts.ForEach(async c => await _concertRepository.InsertAsync(c));
        await _concertRepository.SaveChangesAsync();

        List<ConcertSummaryDto> concertSummaries = _mapper.Map<List<ConcertSummaryDto>>(concerts);

        return concertSummaries;
    }
}
