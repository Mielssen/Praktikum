using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Praktikum542.DTOs;
using Praktikum542.Services;
using System.Security.Claims;

namespace Praktikum542.Controllers
{
    /// <summary>
    /// Керування відгуками та рейтингами турів.
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
        /// Створює відгук для завершеного туру.
        /// </summary>
        /// <remarks>
        /// Відгук може залишити лише авторизований користувач
        /// для власного завершеного бронювання.
        ///
        /// Оцінка повинна бути від 1 до 5.
        ///
        /// Для одного бронювання можна залишити лише один відгук.
        ///
        /// Приклад запиту:
        ///
        ///     POST /api/reviews
        ///     {
        ///        "bookingId": 1,
        ///        "rating": 5,
        ///        "comment": "Чудова подорож!"
        ///     }
        ///
        /// Приклад успішної відповіді:
        ///
        ///     "Відгук успішно додано"
        /// </remarks>
        /// <param name="dto">
        /// Дані відгуку: ідентифікатор бронювання,
        /// оцінка від 1 до 5 та текст коментаря.
        /// </param>
        /// <response code="200">Відгук успішно створено.</response>
        /// <response code="400">
        /// Некоректна оцінка, тур ще не завершився,
        /// бронювання не знайдено або відгук уже існує.
        /// </response>
        /// <response code="401">Користувач не авторизований.</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Create([FromBody] CreateReviewDto dto)
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            _service.Create(credentialId, dto);

            return Ok("Відгук успішно додано");
        }

        /// <summary>
        /// Отримує всі відгуки для конкретного туру.
        /// </summary>
        /// <param name="tourId">Ідентифікатор туру.</param>
        /// <response code="200">Список відгуків для туру.</response>
        [HttpGet("tour/{tourId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetByTour(int tourId)
        {
            return Ok(_service.GetByTourId(tourId));
        }

        /// <summary>
        /// Отримує середній рейтинг конкретного туру.
        /// </summary>
        /// <param name="tourId">Ідентифікатор туру.</param>
        /// <response code="200">Середній рейтинг туру.</response>
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
    }
}