using BusinessObject.DTO.RoomItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IRoomPlacementService
    {
        Task<List<RoomItemListItemDto>> ListAsync(Guid roomId);
        Task UpsertAsync(Guid roomId, UpsertRoomItemsRequest req);
        Task RemoveAsync(Guid roomId, RemoveRoomItemsRequest req);
    }

}
