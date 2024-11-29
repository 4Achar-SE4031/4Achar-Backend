using System.ComponentModel.DataAnnotations;

namespace Concertify.Domain.Dtos.Concert;

public record ConcertIdDto
{
    [Required]
    public int Id { get; set; }
}
