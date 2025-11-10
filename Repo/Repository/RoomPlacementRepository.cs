
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BusinessObject.DTO.RoomItem;           
using BusinessObject.Models;                
using DataAccess;
using Repo.IRepository;

namespace Repo.Repository
{           
    public class RoomPlacementRepository : IRoomPlacementRepository
    {
        private readonly HomeTrackDBContext _ctx;

        public RoomPlacementRepository(HomeTrackDBContext ctx)
        {
            _ctx = ctx;
        }

        // ========== 1) Lấy list item trong 1 room ==========
        public async Task<List<RoomItemListItemDto>> ListByRoomAsync(Guid roomId)
        {
            // lấy tất cả row trong bảng nối
            var data = await _ctx.RoomItemInRooms
                .Where(x => x.RoomId == roomId)
                .Include(x => x.RoomItem)          // để lấy thêm tên item gốc
                .AsNoTracking()
                .ToListAsync();

            // map sang DTO của bạn (đang dùng Id, Name)
            var result = data.Select(x => new RoomItemListItemDto
            {
                // DTO của bạn đang dùng Id tự tạo -> mình map RoomItemId vào Id
                Id = x.RoomItemId,
                Name = x.RoomItem != null ? x.RoomItem.Item : null,
                SubName = x.RoomItem?.SubName,
                RoomType = x.RoomItem?.RoomType,
                // toạ độ từ bảng nối
                X = x.X,
                Y = x.Y
            }).ToList();

            return result;
        }

        public async Task UpsertPlacementsAsync(Guid roomId, List<RoomItemUpsertDto> items, bool replaceAll)
        {
            items ??= new List<RoomItemUpsertDto>();

            // lấy RoomItemId mà client gửi lên
            var itemIds = items.Select(i => i.ItemId).Distinct().ToList();

            // kiểm tra xem trong DB có những cái nào thật sự tồn tại
            var existingItemIds = await _ctx.RoomItems
                .Where(x => itemIds.Contains(x.RoomItemId))
                .Select(x => x.RoomItemId)
                .ToListAsync();

            // nếu có cái nào không tồn tại thì báo luôn cho dễ debug
            var notFound = itemIds.Except(existingItemIds).ToList();
            if (notFound.Any())
            {
                // bạn có thể throw hoặc return lỗi rõ ràng
                throw new ArgumentException("Các itemId sau không tồn tại trong RoomItem: "
                    + string.Join(", ", notFound));
            }

            // đoạn dưới giữ nguyên như mình viết trước
            var existing = await _ctx.RoomItemInRooms
                .Where(x => x.RoomId == roomId)
                .ToListAsync();

            if (replaceAll)
            {
                if (existing.Count > 0)
                {
                    _ctx.RoomItemInRooms.RemoveRange(existing);
                    await _ctx.SaveChangesAsync();
                }
                existing.Clear();
            }

            var now = DateTime.UtcNow;

            foreach (var dto in items)
            {
                var placement = existing.FirstOrDefault(x => x.RoomItemId == dto.ItemId);

                if (placement == null)
                {
                    placement = new RoomItemInRoom
                    {
                        RoomId = roomId,
                        RoomItemId = dto.ItemId,
                        X = dto.X ?? 0,
                        Y = dto.Y ?? 0,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _ctx.RoomItemInRooms.Add(placement);
                }
                else
                {
                    placement.X = dto.X ?? placement.X;
                    placement.Y = dto.Y ?? placement.Y;
                    placement.UpdatedAt = now;
                }
            }

            await _ctx.SaveChangesAsync();
        }

        // ========== 3) Xoá một số item ra khỏi room ==========
        public async Task RemovePlacementsAsync(Guid roomId, List<Guid> itemIds)
        {
            if (itemIds == null || itemIds.Count == 0)
                return;

            var targets = await _ctx.RoomItemInRooms
                .Where(x => x.RoomId == roomId && itemIds.Contains(x.RoomItemId))
                .ToListAsync();

            if (targets.Count == 0)
                return;

            _ctx.RoomItemInRooms.RemoveRange(targets);
            await _ctx.SaveChangesAsync();
        }

        public async Task<Guid> GetDefaultRoomForUserAsync(Guid userId, CancellationToken ct = default)
        {
            // 1) Ưu tiên một Room đang có placement
            var roomWithPlacement = await _ctx.RoomItemInRooms
                .Select(p => p.RoomId)
                .FirstOrDefaultAsync(ct);

            if (roomWithPlacement != Guid.Empty)
                return roomWithPlacement;

            // 2) Nếu chưa có placement nào, trả về bất kỳ Room nào có trong DB
            var anyRoom = await _ctx.Rooms
                .Select(r => r.RoomId)
                .FirstOrDefaultAsync(ct);

            return anyRoom; // có thể Guid.Empty nếu DB chưa có room
        }



        public async Task<List<RoomItemInRoomDto>> GetPlacementsAsync(Guid roomId, CancellationToken ct = default)
        {
            return await _ctx.RoomItemInRooms
                .Where(p => p.RoomId == roomId)
                .Select(p => new RoomItemInRoomDto
                {
                    RoomItemId = p.RoomItemId,
                    Name = p.RoomItem != null ? p.RoomItem.Item : null,
                    SubName = p.RoomItem != null ? p.RoomItem.SubName : null,
                    X = p.X, // hoặc p.X ?? 0m nếu muốn luôn non-null
                    Y = p.Y
                })
                .AsNoTracking()
                .ToListAsync(ct);
        }



        public async Task UpdatePositionAsync(Guid roomId, Guid roomItemId, decimal x, decimal y, CancellationToken ct = default)
        {
            var placement = await _ctx.RoomItemInRooms
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.RoomItemId == roomItemId, ct);

            if (placement == null)
                throw new InvalidOperationException("Placement không tồn tại trong phòng.");

            placement.X = x;
            placement.Y = y;
            placement.UpdatedAt = DateTime.UtcNow;

            //await _ctx.SaveChangesAsync(ct); // commit tại đây
        }


        public Task SaveChangesAsync(CancellationToken ct = default)
            => _ctx.SaveChangesAsync(ct);
    }


}
