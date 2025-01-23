namespace Concertify.Domain.Dtos
{
    /// <summary>
    /// DTO for updating an existing comment (editing its text).
    /// </summary>
    public class UpdateCommentDto
    {
        public string NewText { get; set; } = default!;
    }
}
