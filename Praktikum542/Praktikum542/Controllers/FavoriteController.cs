using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Services;
using System.Security.Claims;

namespace Praktikum542.Controllers
{
    /// <summary>
    /// Керує списком обраних турів користувача.
    /// </summary>
    [Route("api/favorites")]
    [ApiController]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly FavoriteService _service;

        public FavoriteController(FavoriteService service)
        {
            _service = service;
        }

        /// <summary>
        /// Отримує список обраних турів поточного авторизованого користувача.
        /// </summary>
        /// <remarks>
        /// Приклад відповіді:
        ///
        ///     [
        ///       {
        ///         "favoriteId": 1,
        ///         "tourId": 3,
        ///         "tourName": "Карпатська казка",
        ///         "description": "Пішохідні маршрути Карпатами",
        ///         "price": 4200,
        ///         "durationDays": 7,
        ///         "createdAt": "2026-09-20T14:32:00"
        ///       }
        ///     ]
        /// </remarks>
        /// <returns>Список обраних турів.</returns>
        /// <response code="200">Список успішно отримано (може бути порожнім).</response>
        /// <response code="401">Користувач не авторизований.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<FavoriteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMy()
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            return Ok(_service.GetMy(credentialId));
        }

        /// <summary>
        /// Додає тур до обраного поточного користувача.
        /// </summary>
        /// <remarks>
        /// Приклад тіла запиту:
        ///
        ///     {
        ///       "tourId": 3
        ///     }
        /// </remarks>
        /// <param name="dto">Ідентифікатор туру, який додається в обране.</param>
        /// <returns>Повідомлення про успішне додавання.</returns>
        /// <response code="200">Тур успішно додано в обране.</response>
        /// <response code="400">
        /// Тур вже перебуває в обраному (код помилки <c>ALREADY_EXISTS</c>)
        /// або тур не знайдено (код помилки <c>NOT_FOUND</c>).
        /// </response>
        /// <response code="401">Користувач не авторизований.</response>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Add([FromBody] AddFavoriteDto dto)
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            _service.Add(credentialId, dto.TourId);
            return Ok("Додано в обране");
        }

        /// <summary>
        /// Видаляє тур зі списку обраного поточного користувача.
        /// </summary>
        /// <param name="tourId">Ідентифікатор туру, який потрібно видалити з обраного.</param>
        /// <returns>Повідомлення про успішне видалення.</returns>
        /// <response code="200">Тур успішно видалено з обраного.</response>
        /// <response code="400">Тур не перебував у обраному (код помилки <c>NOT_FOUND</c>).</response>
        /// <response code="401">Користувач не авторизований.</response>
        [HttpDelete("{tourId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Delete(int tourId)
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            _service.Delete(credentialId, tourId);
            return Ok("Видалено з обраного");
        }
    }
}