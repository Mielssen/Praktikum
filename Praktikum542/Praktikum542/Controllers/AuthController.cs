using Microsoft.AspNetCore.Mvc;
using Praktikum542.Repositories;
using Praktikum542.DTOs;
using Praktikum542.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Praktikum542.Controllers
{
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

        [HttpPost("register")]
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

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var token = _authService.Login(dto);
            return Ok(token);
        }

        [Authorize]
        [HttpGet("profile")]
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

        [Authorize]
        [HttpPut("profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var detail = _repo.GetUserDetail(credentialId);

            if (detail == null)
                return NotFound();

            detail.Name = dto.Name;
            detail.Phone = dto.Phone != null
                ? "+38" + dto.Phone
                : detail.Phone;

            detail.PassportData = dto.PassportData;

            _repo.UpdateUserDetail(detail);

            return Ok("Профіль оновлено");
        }
        [Authorize]
        [HttpPut("change-password")]
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
        [Authorize]
        [HttpPost("profile/avatar")]
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
