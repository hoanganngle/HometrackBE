using BusinessObject.DTO.RoomItem;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IRoomItemRepository
    {
        Task<List<RoomItem>> AddAsync(List<RoomItem> items);
        Task<RoomItem> UpdateAsync(RoomItem item);
        Task<bool> DeleteAsync(Guid id);
        Task<RoomItem> GetByIdAsync(Guid id);
        Task<List<RoomItem>> ListAllAsync();

        Task<List<RoomItemListItemDto>> ListInRoomAsync(Guid roomId);
    }
}
