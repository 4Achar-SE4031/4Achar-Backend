using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Concertify.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ConcertController : ControllerBase
{
    private readonly IConcertService _concertService;

    public ConcertController(IConcertService concertService)
    {
        _concertService = concertService;
    }

    [HttpGet]
    [Produces(typeof(List<ConcertSummaryDto>))]
    public async Task<IActionResult> GetConcertsAsync([FromQuery] ConcertFilterDto concertFilter)
    {
        List<ConcertSummaryDto> concerts = await _concertService.GetConcertsAsync(concertFilter);

        return Ok(concerts);
    }

    [HttpGet]
    [Route("{id}")]
    [Produces(typeof(ConcertDetailsDto))]
    public async Task<IActionResult> GetConcertByIdAsync(int id)
    {
        ConcertDetailsDto concert = await _concertService.GetConcertByIdAsync(id);

        return Ok(concert);
    }

    [HttpPost]
    [Route("search")]
    [Produces(typeof(List<ConcertSummaryDto>))]
    public async Task<IActionResult> SearchAsync(ConcertSearchDto concertSearch)
    {
        List<ConcertSummaryDto> concerts = await _concertService.SearchAsync(concertSearch);

        return Ok(concerts);
    }
}
