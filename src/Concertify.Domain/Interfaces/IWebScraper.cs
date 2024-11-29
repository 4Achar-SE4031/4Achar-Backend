using Concertify.Domain.Models;

namespace Concertify.Domain.Interfaces;

public interface IWebScraper
{
    public List<string> ExtractLinks(string url);
    public Concert Scrape(string url);
}
