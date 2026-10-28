using Microsoft.AspNetCore.Mvc;
using Praktikum542.Services;

namespace Praktikum542.Controllers
{
    [ApiController]
    [Route("api/v2/reviews")]
    public class ReviewV2Controller : ControllerBase
    {
        private readonly ReviewService _service;

        public ReviewV2Controller(ReviewService service)
        {
            _service = service;
        }

        [HttpGet("tour/{tourId}")]
        public IActionResult GetByTourV2(int tourId)
        {
            var rawReviews = _service.GetByTourId(tourId);

            return Ok(new
            {
                Version = "v2",
                TourId = tourId,
                TotalReviews = rawReviews.Count,
                Items = rawReviews
            });
        }
    }
}