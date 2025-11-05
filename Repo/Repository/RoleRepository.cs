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
    public class RoleRepository : IRoleRepository
    {
        private readonly HomeTrackDBContext _context;
        public RoleRepository(HomeTrackDBContext db) => _context = db;
        public async Task AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
        }

        public Task<bool> ExistsByNameAsync(string roleName)
        {
            return _context.Roles.AnyAsync(u => u.RoleName == roleName);
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }
    }
}
