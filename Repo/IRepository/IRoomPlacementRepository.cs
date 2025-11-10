using BusinessObject.DTO.RoomItem;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IRoomPlacementRepository
    {
        Task<List<RoomItemListItemDto>> ListByRoomAsync(Guid roomId);

        Task UpsertPlacementsAsync(Guid roomId, List<RoomItemUpsertDto> items, bool replaceAll);

        Task RemovePlacementsAsync(Guid roomId, List<Guid> itemIds);
        Task<Guid> GetDefaultRoomForUserAsync(Guid userId, CancellationToken ct = default);

        Task<List<RoomItemInRoomDto>> GetPlacementsAsync(Guid roomId, CancellationToken ct = default);

        Task UpdatePositionAsync(Guid roomId, Guid roomItemId, decimal x, decimal y, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default); // nếu cần

    }
}
