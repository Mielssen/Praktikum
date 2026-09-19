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
            var result = await _authService.RegisterUser(dto);

            if (dto.Avatar != null && dto.Avatar.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                var extension = Path.GetExtension(dto.Avatar.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Дозволені тільки JPG, JPEG та PNG файли");

                const long maxFileSize = 5 * 1024 * 1024;

                if (dto.Avatar.Length > maxFileSize)
                    return BadRequest("Максимальний розмір файлу — 5 МБ");

                var email = dto.Email.Trim().ToLower();

                var credential = _repo.GetByEmail(email);

                if (credential == null)
                    return NotFound("Користувача не знайдено");

                var detail = _repo.GetUserDetail(credential.CredentialId);

                if (detail == null)
                    return NotFound("Дані користувача не знайдено");

                var avatarsFolder = Path.Combine(
                    _environment.WebRootPath,
                    "avatars"
                );

                if (!Directory.Exists(avatarsFolder))
                    Directory.CreateDirectory(avatarsFolder);

                var fileName = $"{Guid.NewGuid()}{extension}";

                var filePath = Path.Combine(
                    avatarsFolder,
                    fileName
                );

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Avatar.CopyToAsync(stream);
                }

                detail.AvatarUrl = $"/avatars/{fileName}";

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
            detail.Phone = dto.Phone != null ? "+38" + dto.Phone : detail.Phone;
            detail.PassportData = dto.PassportData;

            _repo.UpdateUserDetail(detail);

            return Ok("Профіль оновлено");
        }
        [Authorize]
        [HttpPost("profile/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не вибрано");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Дозволені тільки JPG, JPEG та PNG файли");

            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
                return BadRequest("Максимальний розмір файлу — 5 МБ");

            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var detail = _repo.GetUserDetail(credentialId);

            if (detail == null)
                return NotFound("Користувача не знайдено");
            if (!string.IsNullOrWhiteSpace(detail.AvatarUrl))
            {
                var oldAvatarPath = Path.Combine(
                    _environment.WebRootPath,
                    detail.AvatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                if (System.IO.File.Exists(oldAvatarPath))
                {
                    System.IO.File.Delete(oldAvatarPath);
                }
            }

            var avatarsFolder = Path.Combine(
                _environment.WebRootPath,
                "avatars"
            );

            if (!Directory.Exists(avatarsFolder))
                Directory.CreateDirectory(avatarsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(
                avatarsFolder,
                fileName
            );

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarUrl = $"/avatars/{fileName}";

            detail.AvatarUrl = avatarUrl;

            _repo.UpdateUserDetail(detail);

            return Ok(new
            {
                message = "Аватар успішно завантажено",
                avatarUrl = avatarUrl
            });
        }
    }
}
