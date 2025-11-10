using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IFloorRepository
    {
        Task<IEnumerable<Floor>> GetByHouseIdAsync(Guid houseId);
        Task<Floor?> GetByIdAsync(Guid id);
        Task AddAsync(Floor floor);
        Task UpdateAsync(Floor floor);
        Task DeleteAsync(Floor floor);
        Task SaveAsync();
    }
}
