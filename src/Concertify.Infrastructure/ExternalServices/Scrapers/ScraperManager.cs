using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;
using Concertify.Infrastructure.Data;
using Concertify.Infrastructure.Interfaces;

namespace Concertify.Infrastructure.ExternalServices.Scrapers;

public class ScraperManager(IWebScraper scraper, ApplicationDbContext context) : IScraperManager
{
    private readonly IWebScraper _scraper = scraper;
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Concert>> StartScraping(string url)
    {
        List<Concert> scrapedConcerts = await _scraper.ExtractLinks(url);

        await _context.Concerts.AddRangeAsync(scrapedConcerts);

        return scrapedConcerts;
    }
}
