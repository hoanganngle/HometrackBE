using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetByFloorIdAsync(Guid floorId);
        Task<Room?> GetByIdAsync(Guid id);
        Task<Room> AddAsync(Room room);
        Task<Room?> UpdateAsync(Room room);
        Task<bool> DeleteAsync(Guid id);
    }
}
