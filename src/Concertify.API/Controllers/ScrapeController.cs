using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace Concertify.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ScrapeController(IScraperService scraperService, IWebHostEnvironment webHostEnvironment) : ControllerBase
{
    private readonly IScraperService _scraperService = scraperService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

    [HttpGet]
    [Route("collect")]
    [Produces(typeof(List<ConcertSummaryDto>))]
    public async IAsyncEnumerable<ConcertSummaryDto> Collect()
    {
        await foreach (var concert in _scraperService.Collect())
        {
            concert.CardImage = $"{Request.Scheme}://{Request.Host}{concert.CardImage.Replace(_webHostEnvironment.WebRootPath, "")}";
            yield return concert;
        }
    }
}
