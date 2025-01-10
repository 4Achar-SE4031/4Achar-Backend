﻿using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace Concertify.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ConcertController(IConcertService concertService, IWebHostEnvironment webHostEnvironment) : ControllerBase
{
    private readonly IConcertService _concertService = concertService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

    [HttpGet]
    [Produces(typeof(List<ConcertSummaryDto>))]
    public async Task<IActionResult> GetConcertsAsync([FromQuery] ConcertFilterDto concertFilter)
    {
        List<ConcertSummaryDto> concerts = await _concertService.GetConcertsAsync(concertFilter);
        concerts.ForEach(c => c.CardImage = $"{Request.Scheme}://{Request.Host}{c.CardImage.Replace(_webHostEnvironment.WebRootPath, "")}");

        return Ok(new
        {
            count = concerts.Count,
            data = concerts
        });
    }

    [HttpGet]
    [Route("{id}")]
    [Produces(typeof(ConcertDetailsDto))]
    public async Task<IActionResult> GetConcertByIdAsync(int id)
    {
        ConcertDetailsDto concert = await _concertService.GetConcertByIdAsync(id);
        concert.CoverImage = $"{Request.Scheme}://{Request.Host}{concert.CoverImage.Replace(_webHostEnvironment.WebRootPath, "")}";

        return Ok(concert);
    }

    [HttpPost]
    [Route("search")]
    [Produces(typeof(List<ConcertSummaryDto>))]
    public async Task<IActionResult> SearchAsync(ConcertSearchDto concertSearch)
    {
        List<ConcertSummaryDto> concerts = await _concertService.SearchAsync(concertSearch);
        concerts.ForEach(c => c.CardImage = $"{Request.Scheme}://{Request.Host}{c.CardImage.Replace(_webHostEnvironment.WebRootPath, "")}");

        return Ok(concerts);
    }
}
