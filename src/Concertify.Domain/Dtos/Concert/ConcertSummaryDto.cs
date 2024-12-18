﻿namespace Concertify.Domain.Dtos.Concert;

public record ConcertSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string StartDate { get; set; } = default!;
    public string? City { get; set; }
    public string Category { get; set; } = default!;

}
