using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Praktikum542.Common;
using Praktikum542.DTOs;
using Praktikum542.Services;

namespace Praktikum542.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _service;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AdminService service, ILogger<AdminController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("users")]
        public IActionResult GetAll([FromQuery] string? role, [FromQuery] string? status, [FromQuery] string? search)
        {
            return Ok(_service.GetAllUsers(role, status, search));
        }

        [HttpGet("users/{id:int}")]
        public IActionResult GetById(int id)
        {
            return Ok(_service.GetUserById(id));
        }

        [HttpPut("users/{id:int}/role")]
        public IActionResult UpdateRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _service.UpdateRole(User.GetUserId(), id, dto.Role);
            return Ok("Роль оновлено");
        }

        [HttpPut("users/{id:int}/data")]
        public IActionResult UpdateData(int id, [FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _service.UpdateUserData(User.GetUserId(), id, dto);
            return Ok("Дані оновлено");
        }

        [HttpPut("users/{id:int}/status")]
        public IActionResult SetStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _service.SetUserStatus(User.GetUserId(), id, dto.Status);
            return Ok("Статус оновлено");
        }

        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            return Ok(_service.GetStatistics());
        }
    }
}