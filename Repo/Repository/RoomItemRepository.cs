using BusinessObject.DTO.RoomItem;
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
    public class RoomItemRepository : IRoomItemRepository
    {
        private readonly HomeTrackDBContext _db;
        public RoomItemRepository(HomeTrackDBContext db) { _db = db; }

        public async Task<List<RoomItem>> AddAsync(List<RoomItem> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Danh sách RoomItem không được rỗng.", nameof(items));

            var now = DateTime.UtcNow;
            foreach (var it in items)
            {
                if (it.CreatedAt == default)
                    it.CreatedAt = now;
            }

            await _db.RoomItems.AddRangeAsync(items);
            await _db.SaveChangesAsync();

            return items;
        }

        public async Task<RoomItem> UpdateAsync(RoomItem item)
        {
            var existing = await _db.RoomItems.FirstOrDefaultAsync(x => x.RoomItemId == item.RoomItemId);
            if (existing == null) return null;

            existing.Item = item.Item;
            existing.RoomType = item.RoomType;
            existing.DefaultX = item.DefaultX;
            existing.DefaultY = item.DefaultY;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _db.RoomItems.FindAsync(id);
            if (entity == null) return false;
            _db.RoomItems.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public Task<RoomItem> GetByIdAsync(Guid id)
        {
            return _db.RoomItems.AsNoTracking().FirstOrDefaultAsync(x => x.RoomItemId == id);
        }

        public Task<List<RoomItem>> ListAllAsync()
        {
            return _db.RoomItems.AsNoTracking().OrderBy(x => x.Item).ToListAsync();
        }

        // List theo Room: map sang DTO kèm X/Y của placement
        public Task<List<RoomItemListItemDto>> ListInRoomAsync(Guid roomId)
        {
            return _db.RoomItemInRooms
                .Where(p => p.RoomId == roomId)
                .Select(p => new RoomItemListItemDto
                {
                    Id = p.RoomItemId,
                    RoomType = p.RoomItem.RoomType,
                    Name = p.RoomItem.Item,
                    X = p.X,
                    Y = p.Y
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
