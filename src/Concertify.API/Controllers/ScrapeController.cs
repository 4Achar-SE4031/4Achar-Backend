using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace Concertify.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ScrapeController(IScraperService scraperService) : ControllerBase
{
    private readonly IScraperService _scraperService = scraperService;

    [HttpGet]
    [Route("collect")]
    [Produces(typeof(List<ConcertSummaryDto>))]
    public async Task<IActionResult> Collect()
    {
        List<ConcertSummaryDto> concerts = await _scraperService.Collect();

        return Ok(concerts);
    }
}
