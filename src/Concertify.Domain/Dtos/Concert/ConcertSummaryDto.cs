﻿namespace Concertify.Domain.Dtos.Concert;

public record ConcertSummaryDto
{
    public string Title { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public string? City { get; set; }
    public string Category { get; set; } = default!;
    public string TicketPrice { get; set; } = default!;

}