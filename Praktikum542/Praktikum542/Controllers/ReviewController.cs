using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Praktikum542.Constants;
using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Services;
using System;
using System.Security.Claims;

namespace Praktikum542.Controllers
{
    /// <summary>
    /// Керування відгуками та рейтингами турів (v1).
    /// </summary>
    [Route("api/reviews")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _service;

        public ReviewController(ReviewService service)
        {
            _service = service;
        }

        /// <summary>
        /// Приватний хелпер для отримання ID користувача без int.Parse у кожному методі.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Користувач не авторизований.");
            }

            return userId;
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Create([FromBody] CreateReviewDto dto)
        {
            try
            {
                var credentialId = GetCurrentUserId();
                _service.Create(credentialId, dto);
                return Ok("Відгук успішно додано");
            }
            catch (AppException ex)
            {
                return BadRequest(new { Code = ex.Code, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("tour/{tourId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetByTour(int tourId)
        {
            return Ok(_service.GetByTourId(tourId));
        }

        [HttpGet("tour/{tourId}/average")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAverageRating(int tourId)
        {
            var averageRating = _service.GetAverageRating(tourId);

            return Ok(new
            {
                tourId,
                averageRating
            });
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                bool isDeleted = _service.DeleteReview(id, currentUserId, User.IsInRole(ReviewConstants.AdminRole));
                
                if (!isDeleted)
                    return NotFound(ReviewConstants.ReviewNotFound);

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}