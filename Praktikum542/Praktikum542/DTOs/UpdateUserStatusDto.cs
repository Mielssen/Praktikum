using System.ComponentModel.DataAnnotations;

namespace Praktikum542.DTOs
{
    public class UpdateUserStatusDto
    {
        [Required]
        [RegularExpression("^(active|deleted)$",
        ErrorMessage = "Статус має бути active або deleted")]
        public string Status { get; set; }
    }
}
