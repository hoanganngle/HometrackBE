using BusinessObject.DTO.RoomItem;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using Service.Exceptions;
using Service.IService;

namespace HomeTrackAPI_BE.Controllers
{
    [ApiController]
    [Route("api/room-items")]
    public class RoomItemsController : ControllerBase
    {
        private readonly IRoomItemService _svc;
        public RoomItemsController(IRoomItemService svc) { _svc = svc; }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] List<AddRoomItemRequest> reqs)
        {
            if (reqs == null || reqs.Count == 0)
                return BadRequest("Danh sách item không được rỗng.");

            // gọi service để tạo hàng loạt
            var created = await _svc.CreateAsync(reqs);

            // nếu bạn muốn trả về danh sách item vừa tạo
            return Ok(created);
        }




        // GET /api/room-items/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RoomItemListItemDto>> GetById(Guid id)
        {
            var entity = await _svc.GetAsync(id);
            if (entity == null) return NotFound();
            return new RoomItemListItemDto
            {
                Id = entity.RoomItemId,
                RoomType = entity.RoomType,
                Name = entity.Item,
                X = entity.DefaultX,
                Y = entity.DefaultY
            };
        }

        // GET /api/room-items  (catalog) 
        // GET /api/room-items?roomId=...  (list theo Room, X/Y lấy từ placement)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomItemListItemDto>>> List([FromQuery] Guid? roomId)
        {
            if (roomId.HasValue)
                return await _svc.ListInRoomAsync(roomId.Value);

            return await _svc.ListCatalogAsync();
        }

        // PUT /api/room-items/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomItemRequest req)
        {
            var updated = await _svc.UpdateAsync(id, req);
            if (updated == null) return NotFound();
            return NoContent();
        }

        // DELETE /api/room-items/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _svc.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }

}
