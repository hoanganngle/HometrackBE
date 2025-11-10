using BusinessObject.DTO.RoomItem;
using BusinessObject.DTO.SubItem;
using BusinessObject.Models;
using Microsoft.AspNetCore.Mvc;
using Service.IService;
using Service.Service;

namespace HomeTrackAPI_BE.Controllers
{
    [ApiController]
    [Route("api")]
    public class SubItemsController : ControllerBase
    {
        private readonly IRoomService _service;

        private readonly IRoomItemService _roomItemService;
        private readonly IRoomPlacementService _roomPlacementService;
        private readonly ISubItemService _subItemService;

        public SubItemsController(
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

        [HttpGet("{roomId:guid}/items/{roomItemId:guid}/subitems")]
        public Task<List<object>> GetSubItems(Guid roomId, Guid roomItemId)
        {
            return _subItemService.ListInPlacementAsync(roomId, roomItemId);
        }

        [HttpPost("{roomId:guid}/items/{roomItemId:guid}/subitems")]
        public async Task<IActionResult> CreateOrLink(
            Guid roomId,
            Guid roomItemId,
            [FromBody] List<LinkOrCreateSubItemDto> list)
        {
            if (list == null || list.Count == 0)
                return BadRequest("Body must contain at least 1 sub item.");

            // lấy từ body luôn (ưu tiên từng item, nhưng có cái default ở đây)
            var first = list.First();
            var houseId = first.HouseId;
            var floorId = first.FloorId;
            var subItemType = first.SubItemType; // NEW: default type
            var color = first.corlor;
            var description = first.description;

            var result = await _subItemService.CreateOrLinkAsync(
                roomId,
                roomItemId,
                list,
                houseId,
                floorId,
                subItemType
            );

            return Ok(result);
        }

    }
}
