using System.ComponentModel.DataAnnotations;

namespace Praktikum542.DTOs
{
    public class UpdateBookingStatusDto
    {
        [Required]
        [RegularExpression("^(pending|confirmed|cancelled)$",
            ErrorMessage = "Статус має бути pending, confirmed або cancelled")]
        public string Status { get; set; }
    }
}