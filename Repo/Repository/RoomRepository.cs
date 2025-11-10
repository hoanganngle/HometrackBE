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
    public class RoomRepository : IRoomRepository
    {
        private readonly HomeTrackDBContext _context;

        public RoomRepository(HomeTrackDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetByFloorIdAsync(Guid floorId)
        {
            var rooms = await _context.Rooms
                .Where(r => r.FloorId == floorId)
                .Include(r => r.Floor)                  // lấy thông tin tầng chứa phòng
                .Include(r => r.RoomItemPlacements)        // lấy danh sách vật phẩm trong room
                    .ThenInclude(ri => ri.RoomItem)     // lấy chi tiết vật phẩm gốc
                .AsSplitQuery()                         // tránh join nặng
                .AsNoTracking()
                .ToListAsync();

            return rooms ?? new List<Room>();
        }


        public async Task<Room?> GetByIdAsync(Guid id)
        {
            return await _context.Rooms
                .Include(r => r.Floor) // lấy thông tin tầng chứa phòng
                .Include(r => r.RoomItemPlacements)   // lấy danh sách các item gắn trong room
                    .ThenInclude(ri => ri.RoomItem) // lấy thông tin RoomItem gốc
                .AsSplitQuery() // tránh join nặng khi có nhiều item
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoomId == id);
        }




        public async Task<Room> AddAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<Room?> UpdateAsync(Room room)
        {
            var existing = await _context.Rooms.FindAsync(room.RoomId);
            if (existing == null) return null;

            existing.Name = room.Name;
            existing.Type = room.Type;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return false;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
