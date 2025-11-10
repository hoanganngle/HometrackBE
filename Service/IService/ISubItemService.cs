using BusinessObject.DTO.RoomItem;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface ISubItemService
    {
        Task<List<object>> ListInPlacementAsync(Guid roomId, Guid roomItemId);
        Task<List<SubItem>> CreateOrLinkAsync(
    Guid roomId,
    Guid roomItemId,
    List<LinkOrCreateSubItemDto> list,
    Guid? houseId,
    Guid? floorId,
    string? defaultSubItemType // NEW
);
    }

}
