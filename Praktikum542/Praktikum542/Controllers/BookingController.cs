using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Services;
using System.Security.Claims;

namespace Praktikum542.Controllers
{
    /// <summary>
    /// Керування бронюваннями турів: створення, перегляд, оновлення статусу та скасування.
    /// </summary>
    [Route("api/bookings")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _service;

        public BookingController(BookingService service)
        {
            _service = service;
        }

        /// <summary>
        /// Створює нове бронювання туру для поточного авторизованого користувача.
        /// </summary>
        /// <remarks>
        /// Потребує заголовок Authorization: Bearer {token}.
        ///
        /// Приклад запиту:
        ///
        ///     POST /api/bookings
        ///     {
        ///        "tourId": 1,
        ///        "startDate": "2026-07-15",
        ///        "personsCount": 2,
        ///        "personIds": [1, 2]
        ///     }
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     "Бронювання створено"
        /// </remarks>
        /// <param name="dto">Дані бронювання: ідентифікатор туру, дата початку, кількість осіб та збережені особи.</param>
        /// <response code="200">Бронювання успішно створено.</response>
        /// <response code="400">Помилка валідації вхідних даних (невірна дата, тур не знайдено тощо).</response>
        /// <response code="401">Користувач не авторизований або токен недійсний.</response>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Create([FromBody] CreateBookingDto dto)
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            _service.Create(credentialId, dto);
            return Ok("Бронювання створено");
        }

        /// <summary>
        /// Оновлює статус існуючого бронювання (доступно тільки менеджерам).
        /// </summary>
        /// <remarks>
        /// Потребує роль 'manager'.
        ///
        /// Дозволені статуси: Pending, Confirmed, Cancelled.
        ///
        /// Приклад запиту:
        ///
        ///     PUT /api/bookings/5/status
        ///     {
        ///        "status": "Confirmed"
        ///     }
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     "Статус оновлено"
        /// </remarks>
        /// <param name="id">Ідентифікатор бронювання.</param>
        /// <param name="dto">Об'єкт із новим статусом бронювання.</param>
        /// <response code="200">Статус бронювання успішно змінено.</response>
        /// <response code="400">Некоректний статус або бронювання не знайдено.</response>
        /// <response code="401">Користувач не авторизований.</response>
        /// <response code="403">Недостатньо прав (потрібна роль 'manager').</response>
        [Authorize(Roles = "manager")]
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult UpdateStatus(int id, [FromBody] UpdateBookingStatusDto dto)
        {
            _service.UpdateStatus(id, dto.Status);
            return Ok("Статус оновлено");
        }

        /// <summary>
        /// Отримує список усіх бронювань у системі з можливістю фільтрації (доступно тільки менеджерам).
        /// </summary>
        /// <remarks>
        /// Потребує роль 'manager'.
        ///
        /// Приклад запиту:
        ///
        ///     GET /api/bookings?status=Confirmed
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     [
        ///        {
        ///           "bookingId": 1,
        ///           "tourId": 3,
        ///           "tourName": "Карпатські стежки",
        ///           "startDate": "2026-07-20",
        ///           "personsCount": 2,
        ///           "totalPrice": 7000.00,
        ///           "status": "Confirmed",
        ///           "bookingDate": "2026-06-01T10:00:00Z",
        ///           "userEmail": "client@example.com",
        ///           "userName": "Іван Петренко"
        ///        }
        ///     ]
        /// </remarks>
        /// <param name="status">Статус для фільтрації списку бронювань (необов'язковий).</param>
        /// <response code="200">Список усіх знайдених бронювань.</response>
        /// <response code="401">Користувач не авторизований.</response>
        /// <response code="403">Недостатньо прав (потрібна роль 'manager').</response>
        [Authorize(Roles = "manager")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetAll(
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var allBookings = _service.GetAll(status);
            var result = allBookings.ApplyPagination(page, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Отримує список бронювань поточного авторизованого користувача.
        /// </summary>
        /// <remarks>
        /// Ідентифікатор користувача визначається автоматично з JWT-токена.
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     [
        ///        {
        ///           "bookingId": 10,
        ///           "tourId": 2,
        ///           "tourName": "Морський бриз",
        ///           "startDate": "2026-08-01",
        ///           "personsCount": 1,
        ///           "totalPrice": 5200.00,
        ///           "status": "Pending",
        ///           "bookingDate": "2026-06-10T14:30:00Z"
        ///        }
        ///     ]
        /// </remarks>
        /// <response code="200">Список бронювань поточного користувача.</response>
        /// <response code="401">Користувач не авторизований.</response>
        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMy()
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            return Ok(_service.GetMy(credentialId));
        }

        /// <summary>
        /// Скасовує бронювання поточним авторизованим користувачем.
        /// </summary>
        /// <remarks>
        /// Користувач може скасувати лише власне бронювання.
        ///
        /// Приклад запиту:
        ///
        ///     DELETE /api/bookings/10
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     "Бронювання скасовано"
        /// </remarks>
        /// <param name="id">Ідентифікатор бронювання для скасування.</param>
        /// <response code="200">Бронювання успішно скасовано.</response>
        /// <response code="400">Бронювання не знайдено або воно не належить поточному користувачу.</response>
        /// <response code="401">Користувач не авторизований.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Cancel(int id)
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            _service.Cancel(credentialId, id);
            return Ok("Бронювання скасовано");
        }
    }
}