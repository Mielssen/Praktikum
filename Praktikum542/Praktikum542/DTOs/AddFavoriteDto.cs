using System.ComponentModel.DataAnnotations;

namespace Praktikum542.DTOs;

public class AddFavoriteDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Некоректний ідентифікатор туру")]
    public int TourId { get; set; }
}