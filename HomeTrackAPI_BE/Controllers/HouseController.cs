using BusinessObject.DTO.House;
using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.IService;
using System.Security.Claims;

namespace HomeTrackAPI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HouseController : ControllerBase
    {
        private readonly IHouseService _houseService;

        public HouseController(IHouseService houseService)
        {
            _houseService = houseService;
        }

        [HttpGet("get-by-user")]
        public async Task<IActionResult> GetByUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId) || userId == Guid.Empty)
                return Unauthorized();

            var houses = await _houseService.GetByUserIdAsync(userId);
            return Ok(houses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var house = await _houseService.GetByIdAsync(id);
            if (house == null) return NotFound();
            return Ok(house);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] HouseRequestDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId) || userId == Guid.Empty)
                return Unauthorized();

            var newHouse = await _houseService.CreateAsync(userId, dto);
            return Ok(newHouse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] HouseRequestDto dto)
        {
            var result = await _houseService.UpdateAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _houseService.DeleteAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
