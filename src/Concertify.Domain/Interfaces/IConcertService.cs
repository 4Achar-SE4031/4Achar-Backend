﻿using Concertify.Domain.Dtos.Concert;

namespace Concertify.Domain.Interfaces;

public interface IConcertService
{
    public Task<List<ConcertSummaryDto>> GetConcertsAsync(ConcertFilterDto concertFilterDto);
    public Task<ConcertDetailsDto> GetConcertByIdAsync(int concertId);
}
