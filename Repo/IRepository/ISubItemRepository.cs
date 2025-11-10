using BusinessObject.DTO.RoomItem;
using BusinessObject.DTO.SubItem;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface ISubItemRepository
    {
        Task<List<SubItem>> ListInPlacementAsync(Guid roomId, Guid roomItemId);

        Task<RoomItemInRoom> GetPlacementAsync(Guid roomId, Guid roomItemId);

        Task<bool> RoomItemExistsAsync(Guid roomItemId);
        Task<bool> IsRoomItemPlacedSomewhereAsync(Guid roomItemId);

        Task<SubItem> GetSubItemAsync(Guid subItemId, Guid roomId, Guid roomItemId);

        Task AddSubItemAsync(SubItem subItem);

        Task<SubItem> UpdateAsync(Guid subItemId, UpdateSubItemDto dto);

        Task RemoveInPlacementAsync(Guid roomId, Guid roomItemId, List<Guid> subItemIds);

        Task ReorderAsync(Guid roomId, Guid roomItemId, List<Guid> orderedSubItemIds);

        Task SaveChangesAsync();
    }

}
