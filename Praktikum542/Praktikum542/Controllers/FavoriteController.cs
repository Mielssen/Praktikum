using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Praktikum542.DTOs;
using Praktikum542.Services;

namespace Praktikum542.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly FavoriteService _service;

        public FavoriteController(FavoriteService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetMy()
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            return Ok(_service.GetMy(credentialId));
        }

        [HttpPost]
        public IActionResult Add([FromBody] AddFavoriteDto dto)
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            _service.Add(credentialId, dto.TourId);
            return Ok("Додано в обране");
        }

        [HttpDelete("{tourId}")]
        public IActionResult Delete(int tourId)
        {
            var credentialId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            _service.Delete(credentialId, tourId);
            return Ok("Видалено з обраного");
        }
    }
}