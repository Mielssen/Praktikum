using System.ComponentModel.DataAnnotations;

namespace Praktikum542.DTOs
{
    public class CreateTourDto : IValidatableObject
    {
        [Required(ErrorMessage = "Назва туру обов'язкова")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Ціна обов'язкова")]
        [Range(0, 1000000, ErrorMessage = "Некоректна ціна")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Тривалість обов'язкова")]
        [Range(1, 365, ErrorMessage = "Тривалість має бути від 1 до 365 днів")]
        public int DurationDays { get; set; }

        [Required(ErrorMessage = "Дата початку обов'язкова")]
        public DateOnly AvailableFrom { get; set; }

        [Required(ErrorMessage = "Дата завершення обов'язкова")]
        public DateOnly AvailableTo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Некоректний тип туру")]
        public int TypeId { get; set; }
      

        public List<TourAssetDto>? Assets { get; set; }



        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AvailableFrom == default)
                yield return new ValidationResult(
                    "Дата початку обов'язкова",
                    new[] { nameof(AvailableFrom) });

            if (AvailableTo == default)
                yield return new ValidationResult(
                    "Дата завершення обов'язкова",
                    new[] { nameof(AvailableTo) });

            if (AvailableFrom != default &&
                AvailableTo != default &&
                AvailableTo < AvailableFrom)
            {
                yield return new ValidationResult(
                    "Дата завершення не може бути раніше дати початку",
                    new[] { nameof(AvailableTo) });
            }
        }

    }



}
