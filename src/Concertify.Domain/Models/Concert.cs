﻿namespace Concertify.Domain.Models;

public class Concert : EntityBase
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime StartDateTime { get; set; } = default!;
    public string? City { get; set; }
    public string Location { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string TicketPrice { get;set; } = default!;
    public float Latitude { get; set; } = default!;
    public float Longitude { get; set; } = default!;
    public string CoverImage { get; set;} = default!;
    public string CardImage { get; set; } = default!;
    public string Url { get; set; } = default!;

}
