using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;
using Concertify.Infrastructure.Data;
using Concertify.Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Concertify.Infrastructure.ExternalServices.Scrapers;

public class ScraperManager(IWebScraper scraper, ApplicationDbContext context) : IScraperManager
{
    private readonly IWebScraper _scraper = scraper;
    private readonly ApplicationDbContext _context = context;

    public async IAsyncEnumerable<Concert> StartScraping(string url)
    {
        await foreach (var concert in _scraper.ExtractLinks(url))
        {
            bool exists = await _context.Concerts.AnyAsync(c => c.Title == concert.Title && c.StartDateTime == concert.StartDateTime && c.City == concert.City);
            if (exists)
                continue;
            await _context.Concerts.AddAsync(concert);
            await _context.SaveChangesAsync();
            yield return concert;

        }
    }
}
