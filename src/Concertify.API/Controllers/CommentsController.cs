using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Concertify.Domain.Dtos;
using Concertify.Domain.Interfaces;

namespace Concertify.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // -----------------------------
        // 1) GET: /api/comments/event/{eventId}
        // -----------------------------
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetCommentsForEvent(int eventId)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var comments = await _commentService.GetCommentsForEventAsync(eventId, currentUserId);

            return Ok(new { comments });
        }

        // -----------------------------
        // 2) POST: /api/comments
        //    (Create new comment or reply)
        // -----------------------------
        [HttpPost]
        [Authorize]  // Require auth to post
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest("Comment text cannot be empty.");

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return new UnauthorizedObjectResult("No valid user logged in.");

            try
            {
                var commentDto = await _commentService.CreateCommentAsync(dto, currentUserId);
                return Ok(commentDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating comment: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the comment.");
            }
        }

        // -----------------------------
        // 3) DELETE: /api/comments/{commentId}
        // -----------------------------
        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return new UnauthorizedObjectResult("No valid user logged in.");

            try
            {
                await _commentService.DeleteCommentAsync(commentId, currentUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting comment: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the comment.");
            }
        }

        // -----------------------------
        // 4) PUT: /api/comments/{commentId}
        //    (Update/edit a comment's text)
        // -----------------------------
        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.NewText))
                return BadRequest("New text cannot be empty.");

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return new UnauthorizedObjectResult("No valid user logged in.");

            try
            {
                var updatedComment = await _commentService.UpdateCommentAsync(commentId, dto, currentUserId);
                return Ok(updatedComment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating comment: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the comment.");
            }
        }

        // -----------------------------
        // 5) POST: /api/comments/{commentId}/toggle-like
        //    (Toggle user like on a comment)
        // -----------------------------
        [HttpPost("{commentId}/toggle-like")]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int commentId)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return new UnauthorizedObjectResult("No valid user logged in.");

            try
            {
                var updatedComment = await _commentService.ToggleLikeAsync(commentId, currentUserId);
                return Ok(updatedComment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling like: {ex.Message}");
                return StatusCode(500, "An error occurred while toggling the like.");
            }
        }
    }
}
