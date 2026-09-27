namespace Praktikum542.DTOs
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }

        public int BookingId { get; set; }

        public int TourId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? UserName { get; set; }
    }
}