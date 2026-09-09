using System.ComponentModel.DataAnnotations;

namespace Praktikum542.DTOs
{
    public class UpdateUserRoleDto
    {
        [Required]
        [RegularExpression("^(client|manager|admin)$",
        ErrorMessage = "Роль має бути client, manager або admin")]
        public string Role { get; set; }
    }
}
