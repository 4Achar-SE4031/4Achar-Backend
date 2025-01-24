namespace Concertify.Domain.Dtos.Concert;

public record ConcertRatingDto
{
    public string UserId { get; set; }
    public int ConcertId { get; set; }
    public float Rating { get; set; }
}
