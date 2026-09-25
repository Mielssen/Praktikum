namespace Praktikum542.DTOs
{
    public class FavoriteDto
    {
        public int FavoriteId { get; set; }
        public int TourId { get; set; }
        public string TourName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}