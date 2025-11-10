using BusinessObject.DTO.RoomItem;
using BusinessObject.DTO.SubItem;
using BusinessObject.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repo.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.Repository
{
    public class SubItemRepository : ISubItemRepository
    {
        private readonly HomeTrackDBContext _db;

        public SubItemRepository(HomeTrackDBContext db)
        {
            _db = db;
        }

        public Task<List<SubItem>> ListInPlacementAsync(Guid roomId, Guid roomItemId)
        {
            return _db.SubItems
                .Where(s => s.RoomId == roomId && s.RoomItemId == roomItemId)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.CreatedAt)
                .ToListAsync();
        }

        public Task<RoomItemInRoom> GetPlacementAsync(Guid roomId, Guid roomItemId)
        {
            return _db.RoomItemInRooms
                .FirstOrDefaultAsync(x => x.RoomId == roomId && x.RoomItemId == roomItemId);
        }

        public Task<bool> RoomItemExistsAsync(Guid roomItemId)
        {
            return _db.RoomItems.AnyAsync(x => x.RoomItemId == roomItemId);
        }

        public Task<bool> IsRoomItemPlacedSomewhereAsync(Guid roomItemId)
        {
            return _db.RoomItemInRooms.AnyAsync(x => x.RoomItemId == roomItemId);
        }

        public Task<SubItem> GetSubItemAsync(Guid subItemId, Guid roomId, Guid roomItemId)
        {
            return _db.SubItems
                .FirstOrDefaultAsync(s =>
                    s.SubItemId == subItemId &&
                    s.RoomId == roomId &&
                    s.RoomItemId == roomItemId);
        }

        public Task AddSubItemAsync(SubItem subItem)
        {
            _db.SubItems.Add(subItem);
            return Task.CompletedTask;
        }

        public async Task<SubItem> UpdateAsync(Guid subItemId, UpdateSubItemDto dto)
        {
            var sub = await _db.SubItems.FirstOrDefaultAsync(s => s.SubItemId == subItemId);
            if (sub == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                sub.Name = dto.Name.Trim();

            if (dto.SortOrder.HasValue)
                sub.SortOrder = dto.SortOrder.Value;

            if (dto.ImageUri != null)
                sub.ImageUri = dto.ImageUri;

            sub.UpdatedAt = DateTime.UtcNow;

            return sub;
        }

        public async Task RemoveInPlacementAsync(Guid roomId, Guid roomItemId, List<Guid> subItemIds)
        {
            var subs = await _db.SubItems
                .Where(s => s.RoomId == roomId
                         && s.RoomItemId == roomItemId
                         && subItemIds.Contains(s.SubItemId))
                .ToListAsync();

            _db.SubItems.RemoveRange(subs);
        }

        public async Task ReorderAsync(Guid roomId, Guid roomItemId, List<Guid> orderedSubItemIds)
        {
            // lấy toàn bộ subitem của placement này
            var subs = await _db.SubItems
                .Where(s => s.RoomId == roomId && s.RoomItemId == roomItemId)
                .ToListAsync();

            // map thứ tự mới
            int order = 0;
            foreach (var id in orderedSubItemIds)
            {
                var sub = subs.FirstOrDefault(x => x.SubItemId == id);
                if (sub != null)
                {
                    sub.SortOrder = order++;
                    sub.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }

}
