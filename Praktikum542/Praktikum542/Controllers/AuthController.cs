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
        private readonly IWebHostEnvironment _environment;

        public AuthController(
       CredentialsRepository repo,
       AuthentificationService authService,
       IWebHostEnvironment environment)
        {
            _repo = repo;
            _authService = authService;
            _environment = environment;
        }

        /// <summary>
        /// Реєструє нового користувача (роль "client"), дозволяє додати аватар
        /// та одразу видає JWT-токен.
        /// </summary>
        /// <remarks>
        /// Дані передаються у форматі multipart/form-data.
        ///
        /// Поля:
        /// - Email — email користувача
        /// - Password — пароль
        /// - Name — ім'я
        /// - Phone — номер телефону без +38
        /// - PassportData — паспортні дані
        /// - DateOfBirth — дата народження
        /// - Avatar — необов'язкове фото профілю
        ///
        /// Для аватара дозволені формати JPG, JPEG та PNG.
        /// Максимальний розмір аватара — 5 МБ.
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
        /// - Аватар необов'язковий
        /// </remarks>
        /// <param name="dto">
        /// Дані для реєстрації та необов'язковий файл аватара
        /// </param>
        /// <response code="200">
        /// Реєстрація успішна, повертає JWT-токен
        /// </response>
        /// <response code="400">
        /// Помилка валідації даних або аватара
        /// </response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(TokenResponceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromForm] RegisterRequestDto dto)
        {
            if (dto.Avatar != null && dto.Avatar.Length > 0)
            {
                var validationError = ValidateAvatar(dto.Avatar);

                if (validationError != null)
                    return BadRequest(validationError);
            }

            var result = await _authService.RegisterUser(dto);

            if (dto.Avatar != null && dto.Avatar.Length > 0)
            {
                var email = dto.Email.Trim().ToLower();

                var credential = _repo.GetByEmail(email);

                if (credential == null)
                    return NotFound("Користувача не знайдено");

                var detail = _repo.GetUserDetail(credential.CredentialId);

                if (detail == null)
                    return NotFound("Дані користувача не знайдено");

                var avatarUrl = await SaveAvatarAsync(dto.Avatar);

                detail.AvatarUrl = avatarUrl;

                _repo.UpdateUserDetail(detail);
            }

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
        /// Змінює пароль поточного авторизованого користувача.
        /// Перед зміною перевіряє правильність старого пароля.
        /// </summary>
        /// <remarks>
        /// Потребує заголовок Authorization: Bearer {token}.
        ///
        /// Приклад запиту:
        ///
        ///     PUT /api/auth/change-password
        ///     {
        ///        "oldPassword": "OldPassword123",
        ///        "newPassword": "NewPassword123",
        ///        "confirmPassword": "NewPassword123"
        ///     }
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     {
        ///        "message": "Пароль успішно змінено"
        ///     }
        ///
        /// Новий пароль не може співпадати зі старим.
        /// Новий пароль та його підтвердження повинні співпадати.
        /// </remarks>
        /// <param name="dto">
        /// Старий пароль, новий пароль та підтвердження нового пароля
        /// </param>
        /// <response code="200">Пароль успішно змінено</response>
        /// <response code="400">
        /// Помилка зміни пароля. Можливі коди:
        /// INVALID_PASSWORD, PASSWORD_MISMATCH, WRONG_PASSWORD, SAME_PASSWORD
        /// </response>
        /// <response code="401">Відсутній або недійсний JWT-токен</response>
        /// <response code="404">Користувача не знайдено</response>
        [Authorize]
        [HttpPut("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            _authService.ChangePassword(credentialId, dto);

            return Ok(new
            {
                message = "Пароль успішно змінено"
            });
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
        ///        "dateOfBirth": "1995-03-15",
        ///        "avatarUrl": "/avatars/550e8400-e29b-41d4-a716-446655440000.jpg"
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
                DateOfBirth = detail.DateOfBirth,
                AvatarUrl = detail.AvatarUrl
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
        /// Завантажує або замінює аватар поточного користувача.
        /// </summary>
        /// <remarks>
        /// Потребує заголовок Authorization: Bearer {token}.
        ///
        /// Файл передається у форматі multipart/form-data.
        ///
        /// Дозволені формати:
        /// - JPG
        /// - JPEG
        /// - PNG
        ///
        /// Максимальний розмір файлу — 5 МБ.
        ///
        /// Приклад успішної відповіді (200):
        ///
        ///     {
        ///        "message": "Аватар успішно завантажено",
        ///        "avatarUrl": "/avatars/550e8400-e29b-41d4-a716-446655440000.jpg"
        ///     }
        ///
        /// Якщо у користувача вже був аватар, старий файл буде видалено.
        /// </remarks>
        /// <param name="file">Файл зображення аватара</param>
        /// <response code="200">Аватар успішно завантажено або замінено</response>
        /// <response code="400">Файл відсутній, має неправильний формат або перевищує 5 МБ</response>
        /// <response code="401">Відсутній або недійсний JWT-токен</response>
        /// <response code="404">Дані користувача не знайдено</response>
        [Authorize]
        [HttpPost("profile/avatar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var validationError = ValidateAvatar(file);

            if (validationError != null)
                return BadRequest(validationError);

            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var detail = _repo.GetUserDetail(credentialId);

            if (detail == null)
                return NotFound("Користувача не знайдено");

            var avatarUrl = await SaveAvatarAsync(
                file,
                detail.AvatarUrl
            );

            detail.AvatarUrl = avatarUrl;

            _repo.UpdateUserDetail(detail);

            return Ok(new
            {
                message = "Аватар успішно завантажено",
                avatarUrl = avatarUrl
            });
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
        private string? ValidateAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Файл не вибрано";

            var allowedExtensions = new[]
            {
        ".jpg",
        ".jpeg",
        ".png"
    };

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return "Дозволені тільки JPG, JPEG та PNG файли";

            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
                return "Максимальний розмір файлу — 5 МБ";

            return null;
        }
        private async Task<string> SaveAvatarAsync(
    IFormFile file,
    string? oldAvatarUrl = null)
        {
            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            var avatarsFolder = Path.Combine(
                _environment.WebRootPath,
                "avatars"
            );

            if (!Directory.Exists(avatarsFolder))
            {
                Directory.CreateDirectory(avatarsFolder);
            }

            if (!string.IsNullOrWhiteSpace(oldAvatarUrl))
            {
                var oldAvatarPath = Path.Combine(
                    _environment.WebRootPath,
                    oldAvatarUrl
                        .TrimStart('/')
                        .Replace('/', Path.DirectorySeparatorChar)
                );

                if (System.IO.File.Exists(oldAvatarPath))
                {
                    System.IO.File.Delete(oldAvatarPath);
                }
            }

            var fileName = $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(
                avatarsFolder,
                fileName
            );

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/avatars/{fileName}";
        }
    }
}
