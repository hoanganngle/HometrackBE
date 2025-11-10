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
    public class FloorRepository : IFloorRepository
    {
        private readonly HomeTrackDBContext _context;

        public FloorRepository(HomeTrackDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Floor>> GetByHouseIdAsync(Guid houseId)
        {
            return await _context.Floors
                .Where(f => f.HouseId == houseId)
                .Include(f => f.House)
                .Include(f => f.Rooms)
                    .ThenInclude(r => r.RoomItemPlacements) // lấy các vật phẩm trong Room
                        .ThenInclude(ri => ri.RoomItem)  // lấy chi tiết RoomItem
                .OrderBy(f => f.Level)
                .AsSplitQuery() // tránh join nặng khi có nhiều tầng và phòng
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<Floor?> GetByIdAsync(Guid id)
        {
            return await _context.Floors
                .Include(f => f.House) // load thông tin House
                .Include(f => f.Rooms)
                    .ThenInclude(r => r.RoomItemPlacements) // load danh sách vật phẩm trong Room
                        .ThenInclude(ri => ri.RoomItem)  // load chi tiết RoomItem
                .AsSplitQuery()         // tránh join nặng khi có nhiều include
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FloorId == id);
        }



        public async Task AddAsync(Floor floor)
        {
            await _context.Floors.AddAsync(floor);
        }

        public async Task UpdateAsync(Floor floor)
        {
            _context.Floors.Update(floor);
        }

        public async Task DeleteAsync(Floor floor)
        {
            _context.Floors.Remove(floor);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
