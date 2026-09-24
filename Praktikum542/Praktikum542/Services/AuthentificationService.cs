using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;

namespace Praktikum542.Services
{
    public class AuthentificationService
    {
        private readonly CredentialsRepository _repo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthentificationService> _logger;

        public AuthentificationService(CredentialsRepository repo, IConfiguration configuration, ILogger<AuthentificationService> logger)
        {
            _repo = repo;
            _configuration = configuration;
            _logger = logger;
        }

        private void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new AppException("INVALID_EMAIL", "Email обов'язковий");

            try
            {
                var addr = new MailAddress(email);
            }
            catch
            {
                throw new AppException("INVALID_EMAIL", "Невірний формат пошти");
            }
        }   

        private string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new AppException("INVALID_PHONE", "Номер телефона обов'язковий");

            phone = phone.Trim();

            if (phone.Length != 10 || !phone.All(char.IsDigit))
                throw new AppException("INVALID_PHONE", "Телефон має містити рівно 10 цифр");

            return "+38" + phone;
        }

        private void ValidateAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var ageLimit = today.AddYears(-18);

            if (dateOfBirth > ageLimit)
                throw new AppException("UNDERAGE", "Користувач повинен бути повнолітнім");
        }

        public string GenerateJwtToken(Credential user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.CredentialId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<TokenResponceDto> RegisterUser(RegisterRequestDto requestDto)
        {
            if (requestDto == null)
                throw new AppException("NULL", "Дані не передані");

            var email = requestDto.Email?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(email))
                throw new AppException("INVALID_EMAIL", "Email обов'язковий");

            if (string.IsNullOrWhiteSpace(requestDto.Password))
                throw new AppException("INVALID_PASSWORD", "Пароль обов'язковий");

            if (string.IsNullOrWhiteSpace(requestDto.Phone))
                throw new AppException("INVALID_PHONE", "Телефон обов'язковий");

            ValidateEmail(email);
            ValidateAge(requestDto.DateOfBirth);

            if (_repo.EmailExists(email))
                throw new AppException("EMAIL_EXISTS", "Така пошта вже зайнята");

            var user = new Credential
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestDto.Password),
                Role = "client",
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            _repo.Register(user); 

            if (user.CredentialId == 0)
                throw new AppException("DB_ERROR", "User not saved correctly");

            var detail = new UserDetail
            {
                CredentialId = user.CredentialId,
                Name = requestDto.Name,
                Phone = NormalizePhone(requestDto.Phone),
                PassportData = requestDto.PassportData,
                DateOfBirth = requestDto.DateOfBirth
            };

            _repo.AddUserDetail(detail);
            _logger.LogInformation("Новий користувач зареєстрований: Email={Email}", email);
            return new TokenResponceDto
            {
                Token = GenerateJwtToken(user)
            };
        }

        public TokenResponceDto Login(LoginDto dto)
        {
            if (dto == null)
                throw new AppException("NULL", "Дані не передані");

            ValidateEmail(dto.Email);

            var email = dto.Email.Trim().ToLower();

            var user = _repo.GetByEmail(email);

            if (user == null)
            {
                _logger.LogWarning("Спроба входу з неіснуючим email: {Email}", dto.Email);
                throw new AppException("NOT_FOUND", "Користувача не знайдено");
            }
            if (user.Status == "deleted")
            {
                _logger.LogWarning("Спроба входу в деактивований акаунт: Email={Email}", dto.Email);
                throw new AppException("ACCOUNT_DISABLED", "Акаунт деактивовано");
            }
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Невірний пароль для Email={Email}", dto.Email);
                throw new AppException("WRONG_PASSWORD", "Неправильний пароль");
            }
            _logger.LogInformation("Успішний вхід: Email={Email}, Role={Role}", user.Email, user.Role);
            return new TokenResponceDto
            {
                Token = GenerateJwtToken(user)
            };
        }
        public void ChangePassword(int credentialId, ChangePasswordDto dto)
        {
            if (dto == null)
                throw new AppException("NULL", "Дані не передані");

            if (string.IsNullOrWhiteSpace(dto.OldPassword))
                throw new AppException("INVALID_PASSWORD", "Старий пароль обов'язковий");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new AppException("INVALID_PASSWORD", "Новий пароль обов'язковий");

            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                throw new AppException("INVALID_PASSWORD", "Підтвердження пароля обов'язкове");

            if (dto.NewPassword != dto.ConfirmPassword)
                throw new AppException("PASSWORD_MISMATCH", "Нові паролі не співпадають");

            var user = _repo.GetById(credentialId);

            if (user == null)
                throw new AppException("NOT_FOUND", "Користувача не знайдено");

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            {
                _logger.LogWarning(
                    "Невірний старий пароль при зміні пароля: CredentialId={CredentialId}",
                    credentialId
                );

                throw new AppException("WRONG_PASSWORD", "Старий пароль введено неправильно");
            }

            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
                throw new AppException(
                    "SAME_PASSWORD",
                    "Новий пароль не може співпадати зі старим"
                );

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            _repo.UpdatePassword(user);

            _logger.LogInformation(
                "Пароль успішно змінено: CredentialId={CredentialId}",
                credentialId
            );
        }
    }

}
