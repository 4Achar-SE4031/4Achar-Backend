using Concertify.Domain.Models;
using Concertify.Infrastructure.Dtos;

namespace Concertify.Infrastructure.Interfaces;

public interface IWebScraper
{
    public IAsyncEnumerable<Concert> ExtractLinks(string url);
    public Task<Concert> Scrape(ScraperContext context);

}
