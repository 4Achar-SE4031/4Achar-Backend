namespace Concertify.Domain.Dtos
{
    /// <summary>
    /// DTO for creating a new comment.
    /// </summary>
    public class CreateCommentDto
    {
        public int EventId { get; set; }
        public string Text { get; set; } = default!;
        public int? ParentId { get; set; }
    }
}
