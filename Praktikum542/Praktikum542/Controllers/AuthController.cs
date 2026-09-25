using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;

namespace Praktikum542.Controllers
{
    /// <summary>
    /// Ендпоінти автентифікації, реєстрації, профілю користувача
    /// та відновлення паролю.
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly CredentialsRepository _repo;
        private readonly AuthentificationService _authService;

        public AuthController(CredentialsRepository repo, AuthentificationService authService)
        {
            _repo = repo;
            _authService = authService;
        }

        /// <summary>
        /// Реєструє нового користувача (роль "client") та одразу видає JWT-токен.
        /// </summary>
        /// <remarks>
        /// Приклад запиту:
        ///
        ///     POST /api/auth/register
        ///     {
        ///        "email": "olena@gmail.com",
        ///        "password": "MySecurePass1",
        ///        "name": "Олена Коваленко",
        ///        "phone": "0501234567",
        ///        "passportData": "123456789",
        ///        "dateOfBirth": "1995-03-15"
        ///     }
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     {
        ///        "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        ///     }
        ///
        /// Вимоги:
        /// - Email має бути унікальним і валідного формату
        /// - Телефон — рівно 10 цифр (без коду країни, +38 додається автоматично)
        /// - Користувач має бути повнолітнім (18+)
        /// </remarks>
        /// <param name="dto">Дані для реєстрації: email, пароль, ім'я, телефон, паспортні дані, дата народження</param>
        /// <response code="200">Реєстрація успішна, повертає JWT-токен</response>
        /// <response code="400">
        /// Помилка валідації. Можливі коди в тілі відповіді:
        /// INVALID_EMAIL, INVALID_PASSWORD, INVALID_PHONE, UNDERAGE, EMAIL_EXISTS, DB_ERROR
        /// </response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(TokenResponceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            var result = await _authService.RegisterUser(dto);
            return Ok(result);
        }

        /// <summary>
        /// Автентифікує користувача за email і паролем, повертає JWT-токен.
        /// </summary>
        /// <remarks>
        /// Приклад запиту:
        ///
        ///     POST /api/auth/login
        ///     {
        ///        "email": "olena@gmail.com",
        ///        "password": "MySecurePass1"
        ///     }
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     {
        ///        "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        ///     }
        ///
        /// Токен містить claims: NameIdentifier (credentialId), Email, Role.
        /// Термін дії токена налаштовується через конфігурацію Jwt:ExpiresInMinutes.
        /// </remarks>
        /// <param name="dto">Email і пароль користувача</param>
        /// <response code="200">Вхід успішний, повертає JWT-токен</response>
        /// <response code="400">
        /// Помилка входу. Можливі коди: INVALID_EMAIL, NOT_FOUND,
        /// ACCOUNT_DISABLED (акаунт деактивований), WRONG_PASSWORD
        /// </response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public IActionResult Login(LoginDto dto)
        {
            var token = _authService.Login(dto);
            return Ok(token);
        }

        /// <summary>
        /// Повертає профіль поточного автентифікованого користувача.
        /// </summary>
        /// <remarks>
        /// Потребує заголовок Authorization: Bearer {token}.
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     {
        ///        "email": "olena@gmail.com",
        ///        "name": "Олена Коваленко",
        ///        "phone": "+380501234567",
        ///        "passportData": "123456789",
        ///        "dateOfBirth": "1995-03-15"
        ///     }
        /// </remarks>
        /// <response code="200">Профіль знайдено і повернено</response>
        /// <response code="401">Відсутній або недійсний JWT-токен</response>
        /// <response code="404">Обліковий запис або дані користувача не знайдено</response>
        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProfile()
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var credential = _repo.GetById(credentialId);
            var detail = _repo.GetUserDetail(credentialId);

            if (credential == null || detail == null)
                return NotFound();

            return Ok(new ProfileDto
            {
                Email = credential.Email,
                Name = detail.Name,
                Phone = detail.Phone,
                PassportData = detail.PassportData,
                DateOfBirth = detail.DateOfBirth
            });
        }

        /// <summary>
        /// Оновлює особисті дані поточного автентифікованого користувача
        /// (ім'я, телефон, паспортні дані).
        /// </summary>
        /// <remarks>
        /// Потребує заголовок Authorization: Bearer {token}.
        ///
        /// Приклад запиту:
        ///
        ///     PUT /api/auth/profile
        ///     {
        ///        "name": "Олена Коваленко",
        ///        "phone": "0501234567",
        ///        "passportData": "123456789"
        ///     }
        ///
        /// Телефон передається без коду країни (10 цифр), +38 додається автоматично.
        /// Email через цей ендпоінт не змінюється.
        /// </remarks>
        /// <param name="dto">Нові значення імені, телефону, паспортних даних</param>
        /// <response code="200">Профіль успішно оновлено</response>
        /// <response code="401">Відсутній або недійсний JWT-токен</response>
        /// <response code="404">Дані користувача не знайдено</response>
        [Authorize]
        [HttpPut("profile")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var detail = _repo.GetUserDetail(credentialId);

            if (detail == null)
                return NotFound();

            detail.Name = dto.Name;
            detail.Phone = dto.Phone != null ? "+38" + dto.Phone : detail.Phone;
            detail.PassportData = dto.PassportData;

            _repo.UpdateUserDetail(detail);

            return Ok("Профіль оновлено");
        }

        /// <summary>
        /// Запускає флоу відновлення паролю: генерує токен скидання
        /// і надсилає посилання на email користувача.
        /// </summary>
        /// <remarks>
        /// Приклад запиту:
        ///
        ///     POST /api/auth/forgot-password
        ///     {
        ///        "email": "olena@gmail.com"
        ///     }
        ///
        /// Приклад відповіді (200), незалежно від того, існує акаунт чи ні:
        ///
        ///     "Якщо email існує, на нього надіслано лист з інструкціями"
        ///
        /// Навмисно завжди повертає однакову відповідь (навіть якщо email
        /// не зареєстрований), щоб унеможливити виявлення існуючих акаунтів
        /// (user enumeration). Токен дійсний 30 хвилин, попередні невикористані
        /// токени користувача при цьому анулюються.
        /// </remarks>
        /// <param name="dto">Email, на який надіслати посилання для скидання паролю</param>
        /// <response code="200">
        /// Запит прийнято. Лист надіслано, якщо такий email зареєстрований
        /// (відповідь однакова в обох випадках)
        /// </response>
        /// <response code="400">Некоректний формат email (код INVALID_EMAIL)</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            await _authService.ForgotPassword(dto);
            return Ok("Якщо email існує, на нього надіслано лист з інструкціями");
        }

        /// <summary>
        /// Встановлює новий пароль за токеном, отриманим з листа відновлення.
        /// </summary>
        /// <remarks>
        /// Приклад запиту:
        ///
        ///     POST /api/auth/reset-password
        ///     {
        ///        "token": "T9_61l-jMIPlvG3RZ5FJT3-GE9GvDMOFQ1_r3WHS8Q4",
        ///        "newPassword": "NewSecurePass123"
        ///     }
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     "Пароль успішно змінено"
        ///
        /// Токен одноразовий — стає недійсним одразу після використання.
        /// Мінімальна довжина нового паролю — 6 символів.
        /// </remarks>
        /// <param name="dto">Токен скидання паролю і новий пароль</param>
        /// <response code="200">Пароль успішно змінено</response>
        /// <response code="400">
        /// Помилка. Можливі коди: INVALID_TOKEN (токен недійсний, прострочений
        /// або вже використаний), INVALID_PASSWORD (менше 6 символів), NOT_FOUND
        /// </response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public IActionResult ResetPassword(ResetPasswordDto dto)
        {
            _authService.ResetPassword(dto);
            return Ok("Пароль успішно змінено");
        }
    }
}