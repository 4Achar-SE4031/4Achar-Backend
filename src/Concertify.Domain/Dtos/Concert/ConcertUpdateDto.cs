namespace Concertify.Domain.Dtos.Concert;

public record ConcertUpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Category { get; set; }
    public string? TicketPrice { get; set; }

}
