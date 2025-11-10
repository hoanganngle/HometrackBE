using BusinessObject.DTO.Floor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.IService;

namespace HomeTrackAPI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FloorController : ControllerBase
    {
        private readonly IFloorService _floorService;

        public FloorController(IFloorService floorService)
        {
            _floorService = floorService;
        }


        [HttpGet("get-by-house/{houseId}")]
        public async Task<IActionResult> GetByHouseId(Guid houseId)
        {
            var floors = await _floorService.GetByHouseIdAsync(houseId);
            return Ok(floors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var floor = await _floorService.GetByIdAsync(id);
            if (floor == null) return NotFound();
            return Ok(floor);
        }

        [HttpPost("create/{houseId}")]
        public async Task<IActionResult> Create(Guid houseId, [FromBody] FloorRequestDto dto)
        {
            var newFloor = await _floorService.CreateAsync(houseId, dto);
            return Ok(newFloor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FloorRequestDto dto)
        {
            var result = await _floorService.UpdateAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _floorService.DeleteAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
