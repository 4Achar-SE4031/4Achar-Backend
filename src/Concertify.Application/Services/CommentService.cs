// File: Concertify.Application/Services/CommentService.cs
using Concertify.Domain.Dtos;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;
using Concertify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concertify.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _dbContext;

        public CommentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CommentDto>> GetCommentsForEventAsync(int eventId, string currentUserId)
        {
            var allComments = await _dbContext.Comments
                .Include(c => c.LikedBy)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.LikedBy)
                .Where(c => c.EventId == eventId)
                .AsNoTracking()
                .ToListAsync();

            // var topLevelComments = allComments
            //     .Where(c => c.ParentId == null)
            //     .OrderBy(c => c.CreatedAt)
            //     .Select(c => MapCommentToDto(c, currentUserId, allComments))
            //     .ToList();

            // return topLevelComments;
               var flatComments = allComments
                .OrderBy(c => c.CreatedAt)
                .Select(c => MapCommentToDto(c, currentUserId, allComments))
                .ToList();
                
                return flatComments;
        }

        public async Task<CommentDto> CreateCommentAsync(CreateCommentDto dto, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                throw new ArgumentException("Comment text cannot be empty.");

            var newComment = new Comment
            {
                Text = dto.Text,
                CreatedAt = DateTime.UtcNow,
                Score = 0,
                UserId = currentUserId,
                EventId = dto.EventId,
                ParentId = dto.ParentId
            };

            _dbContext.Comments.Add(newComment);
            await _dbContext.SaveChangesAsync();

            var commentDto = MapCommentToDto(newComment, currentUserId);
            return commentDto;
        }

        public async Task DeleteCommentAsync(int commentId, string currentUserId)
        {
            var comment = await _dbContext.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found.");

            if (comment.UserId != currentUserId)
                throw new UnauthorizedAccessException("You are not authorized to delete this comment.");

            _dbContext.Comments.Remove(comment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<CommentDto> UpdateCommentAsync(int commentId, UpdateCommentDto dto, string currentUserId)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.NewText))
                throw new ArgumentException("New text cannot be empty.");

            var comment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found.");

            if (comment.UserId != currentUserId)
                throw new UnauthorizedAccessException("You are not authorized to update this comment.");

            comment.Text = dto.NewText;
            await _dbContext.SaveChangesAsync();

            var updatedDto = MapCommentToDto(comment, currentUserId);
            return updatedDto;
        }

        public async Task<CommentDto> ToggleLikeAsync(int commentId, string currentUserId)
        {
            var comment = await _dbContext.Comments
                .Include(c => c.LikedBy)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found.");

            var user = await _dbContext.Users.FindAsync(currentUserId);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid user.");

            if (comment.LikedBy.Any(u => u.Id == currentUserId))
            {
                // Already liked => remove the like
                comment.LikedBy.Remove(user);
                comment.Score = Math.Max(0, comment.Score - 1);
            }
            else
            {
                // Not yet liked => add the user to LikedBy
                comment.LikedBy.Add(user);
                comment.Score += 1;
            }

            await _dbContext.SaveChangesAsync();

            var updatedDto = MapCommentToDto(comment, currentUserId);
            return updatedDto;
        }

        // Helper Methods

        private CommentDto MapCommentToDto(Comment comment, string currentUserId, List<Comment> allComments = null)
        {
            string? replyingToName = null;
            if (comment.Parent != null && comment.Parent.User != null)
            {
                replyingToName = comment.Parent.User.UserName;
            }

            var dto = new CommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                Score = comment.Score,
                UserId = comment.UserId,
                Username = GetUsernameById(comment.UserId),
                ParentId = comment.ParentId,
                EventId = comment.EventId,
                HasLiked = comment.LikedBy.Any(u => u.Id == currentUserId),

                // We'll keep Replies but leave it empty for a flat structure
                Replies = new List<CommentDto>(),

               // Flat structure fields:
               ReplyingTo = comment.ParentId,
               ReplyingToName = replyingToName
            };

            // if (allComments != null)
            // {
            //     var children = allComments
            //         .Where(c => c.ParentId == comment.Id)
            //         .OrderBy(c => c.CreatedAt)
            //         .ToList();

            //     foreach (var child in children)
            //     {
            //         dto.Replies.Add(MapCommentToDto(child, currentUserId, allComments));
            //     }
            // }

            return dto;
        }

        private string GetUsernameById(string userId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            return user?.UserName ?? "UnknownUser";
        }
    }
}
