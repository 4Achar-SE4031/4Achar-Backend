// File: Concertify.Domain/Interfaces/ICommentService.cs
using Concertify.Domain.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Concertify.Domain.Interfaces
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetCommentsForEventAsync(int eventId, string currentUserId);
        Task<CommentDto> CreateCommentAsync(CreateCommentDto dto, string currentUserId);
        Task DeleteCommentAsync(int commentId, string currentUserId);
        Task<CommentDto> UpdateCommentAsync(int commentId, UpdateCommentDto dto, string currentUserId);
        Task<CommentDto> ToggleLikeAsync(int commentId, string currentUserId);
    }
}
