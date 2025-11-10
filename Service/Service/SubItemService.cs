using BusinessObject.DTO.RoomItem;
using BusinessObject.DTO.SubItem;
using BusinessObject.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repo.IRepository;
using Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class SubItemService : ISubItemService
    {
        private readonly ISubItemRepository _subRepo;

        public SubItemService(ISubItemRepository subRepo)
        {
            _subRepo = subRepo;
        }

        public Task<List<object>> ListInPlacementAsync(Guid roomId, Guid roomItemId)
        {
            // Nếu bạn muốn trả object nhẹ, có thể map ở đây; còn không trả thẳng List<SubItem>
            // để giữ đúng chữ ký cũ, mình convert sang object:
            return _subRepo.ListInPlacementAsync(roomId, roomItemId)
                           .ContinueWith(t => t.Result.ConvertAll(s => (object)new
                           {
                               s.SubItemId,
                               s.Name,
                               s.SortOrder,
                               s.ImageUri,
                               s.RoomId,
                               s.RoomItemId
                           }));
        }

        public async Task<List<SubItem>> CreateOrLinkAsync(
                Guid roomId,
                Guid roomItemId,
                List<LinkOrCreateSubItemDto> list,
                Guid? houseId,
                Guid? floorId,
                string? defaultSubItemType // có thể null
            )
        {
            // 1) kiểm tra placement
            var placement = await _subRepo.GetPlacementAsync(roomId, roomItemId);
            if (placement == null)
            {
                var itemExists = await _subRepo.RoomItemExistsAsync(roomItemId);
                if (!itemExists)
                    throw new InvalidOperationException($"RoomItem {roomItemId} not found in catalog.");

                var placedSomewhereElse = await _subRepo.IsRoomItemPlacedSomewhereAsync(roomItemId);
                if (placedSomewhereElse)
                    throw new InvalidOperationException("RoomItem is placed, but NOT in this Room. Check roomId.");

                throw new InvalidOperationException("RoomItem is not placed in this Room.");
            }

            var result = new List<SubItem>();

            foreach (var req in list)
            {
                // chỉ dùng để validate, không gán vào entity
                var effectiveHouseId = req.HouseId ?? houseId;
                var effectiveFloorId = req.FloorId ?? floorId;

                // TODO: chỗ này nếu bạn muốn validate thì làm tại đây
                // if (effectiveHouseId == null) throw ...
                // if (effectiveFloorId == null) throw ...

                if (req.SubItemId.HasValue)
                {
                    // UPDATE
                    var sub = await _subRepo.GetSubItemAsync(
                        req.SubItemId.Value, roomId, roomItemId);

                    if (sub == null)
                        throw new InvalidOperationException("SubItem not found under this placement.");

                    if (!string.IsNullOrWhiteSpace(req.Name))
                        sub.Name = req.Name.Trim();
                    if (req.SortOrder.HasValue)
                        sub.SortOrder = req.SortOrder.Value;
                    if (req.ImageUri != null)
                        sub.ImageUri = req.ImageUri;

                    // NEW: update type nếu FE gửi
                    if (!string.IsNullOrWhiteSpace(req.SubItemType))
                        sub.SubItemType = req.SubItemType!.Trim();

                    sub.UpdatedAt = DateTime.UtcNow;
                    result.Add(sub);
                }
                else
                {
                    // CREATE
                    var sub = new SubItem
                    {
                        SubItemId = Guid.NewGuid(),
                        RoomId = roomId,
                        RoomItemId = roomItemId,
                        Name = (req.Name ?? string.Empty).Trim(),
                        SortOrder = req.SortOrder ?? 0,
                        ImageUri = req.ImageUri,
                        CreatedAt = DateTime.UtcNow,
                        SubItemType = !string.IsNullOrWhiteSpace(req.SubItemType)
                                        ? req.SubItemType!.Trim()
                                        : defaultSubItemType
                    };

                    await _subRepo.AddSubItemAsync(sub);
                    result.Add(sub);
                }
            }

            await _subRepo.SaveChangesAsync();
            return result;
        }

    }
}
