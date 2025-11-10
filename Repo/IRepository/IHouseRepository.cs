using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IHouseRepository
    {
        Task<IEnumerable<House>> GetByUserIdAsync(Guid userId);
        Task<House?> GetByIdAsync(Guid id);
        Task AddAsync(House house);
        Task UpdateAsync(House house);
        Task DeleteAsync(House house);
        Task SaveAsync();
    }
}
