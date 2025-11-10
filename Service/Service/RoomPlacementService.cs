using BusinessObject.DTO.RoomItem;
using Repo.IRepository;
using Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class RoomPlacementService : IRoomPlacementService
    {
        private readonly IRoomPlacementRepository _repo;

        public RoomPlacementService(IRoomPlacementRepository repo)
        {
            _repo = repo;
        }

        public Task<List<RoomItemListItemDto>> ListAsync(Guid roomId)
        {
            return _repo.ListByRoomAsync(roomId);
        }

        public Task UpsertAsync(Guid roomId, UpsertRoomItemsRequest req)
        {
            return _repo.UpsertPlacementsAsync(
                roomId,
                req.Items ?? new List<RoomItemUpsertDto>(),
                req.ReplaceAll
            );
        }

        public Task RemoveAsync(Guid roomId, RemoveRoomItemsRequest req)
        {
            var ids = req?.ItemIds ?? new List<Guid>();
            return _repo.RemovePlacementsAsync(roomId, ids);
        }
    }
}
