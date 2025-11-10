using BusinessObject.DTO.RoomItem;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IRoomItemService
    {
        Task<List<RoomItem>> CreateAsync(IEnumerable<AddRoomItemRequest> reqs);
        Task<RoomItem> UpdateAsync(Guid id, UpdateRoomItemRequest dto);
        Task<bool> DeleteAsync(Guid id);
        Task<RoomItem> GetAsync(Guid id);
        Task<List<RoomItemListItemDto>> ListCatalogAsync();
        Task<List<RoomItemListItemDto>> ListInRoomAsync(Guid roomId);
    }

}
