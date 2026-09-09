using System.ComponentModel.DataAnnotations;

namespace Praktikum542.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯіІїЇєЄ]+$",
            ErrorMessage = "Ім'я має містити тільки літери")]
        public string Name { get; set; }

        [RegularExpression(@"^\d{10}$",
            ErrorMessage = "Телефон має містити рівно 10 цифр")]
        public string? Phone { get; set; }
        public string? PassportData { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }
}