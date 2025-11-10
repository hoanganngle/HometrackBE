using BusinessObject.DTO.Room;
using BusinessObject.DTO.RoomItem;
using BusinessObject.Models;
using Microsoft.AspNetCore.Mvc;
using Service.IService;

namespace HomeTrackAPI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _service;

        private readonly IRoomItemService _roomItemService;
        private readonly IRoomPlacementService _roomPlacementService;
        private readonly ISubItemService _subItemService;

        public RoomController(
            IRoomService service,
            IRoomItemService roomItemService,
            IRoomPlacementService roomPlacementService,
            ISubItemService subItemService   
        )
        {
            _service = service;
            _roomItemService = roomItemService;
            _roomPlacementService = roomPlacementService;
            _subItemService = subItemService;
        }


        [HttpGet("floor/{floorId}")]
        public async Task<IActionResult> GetByFloorId(Guid floorId)
        {
            var result = await _service.GetByFloorIdAsync(floorId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var room = await _service.GetByIdAsync(id);
            if (room == null) return NotFound("Room not found");
            return Ok(room);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddRoomDto dto)
        {
            try
            {
                var room = await _service.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = room.RoomId }, room);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomDto dto)
        {
            try
            {
                var room = await _service.UpdateAsync(id, dto);
                if (room == null) return NotFound("Room not found");
                return Ok(room);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound("Room not found");
            return NoContent();
        }

        [HttpGet("{roomId:guid}/items")]
        public async Task<IActionResult> GetItemsInRoom(Guid roomId)
        {
            var items = await _roomItemService.ListInRoomAsync(roomId);
            return Ok(items);
        }

        [HttpPost("{roomId:guid}/items")]
        public async Task<IActionResult> AddItemsToRoom(Guid roomId, [FromBody] UpsertRoomItemsRequest req)
        {
            await _roomPlacementService.UpsertAsync(roomId, req);

            // lấy lại list để trả về
            var items = await _roomPlacementService.ListAsync(roomId);
            return Ok(items); 
        }


        [HttpPut("{roomId:guid}/items")]
        public async Task<IActionResult> UpsertItemsInRoom(Guid roomId, [FromBody] UpsertRoomItemsRequest req)
        {
            await _roomPlacementService.UpsertAsync(roomId, req);
            return NoContent();
        }

        [HttpDelete("{roomId:guid}/items")]
        public async Task<IActionResult> RemoveItemsFromRoom(Guid roomId, [FromBody] RemoveRoomItemsRequest req)
        {
            await _roomPlacementService.RemoveAsync(roomId, req);
            return NoContent();
        }

        
    }
}
