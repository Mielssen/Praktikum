using System.ComponentModel.DataAnnotations;

namespace Praktikum542.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress(ErrorMessage = "Неправильний формат пошти")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обов'язковий")]
        public string Password { get; set; } = string.Empty;
    }
}