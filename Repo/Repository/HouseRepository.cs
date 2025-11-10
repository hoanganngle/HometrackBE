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
    public class HouseRepository : IHouseRepository
    {
        private readonly HomeTrackDBContext _context;

        public HouseRepository(HomeTrackDBContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<House>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Houses
                .Where(h => h.UserId == userId)
                .Include(h => h.User)
                .Include(h => h.Floors.OrderBy(f => f.Level))
                    .ThenInclude(f => f.Rooms)
                        .ThenInclude(r => r.RoomItemPlacements)
                            .ThenInclude(ri => ri.RoomItem)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<House?> GetByIdAsync(Guid id)
        {
            return await _context.Houses
                .Where(h => h.HouseId == id)
                .Include(h => h.User)
                .Include(h => h.Floors.OrderBy(f => f.Level)) // Lấy tầng theo Level
                    .ThenInclude(f => f.Rooms)
                        .ThenInclude(r => r.RoomItemPlacements)
                            .ThenInclude(ri => ri.RoomItem)   // Lấy chi tiết RoomItem
                .AsSplitQuery() // tránh join quá nặng
                .AsNoTracking() // chỉ đọc dữ liệu
                .FirstOrDefaultAsync();
        }


        public async Task AddAsync(House house)
        {
            await _context.Houses.AddAsync(house);
        }

        public async Task UpdateAsync(House house)
        {
            _context.Houses.Update(house);
        }

        public async Task DeleteAsync(House house)
        {
            _context.Houses.Remove(house);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
