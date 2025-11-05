using DataAccess;
using Repo.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.Repository
{
    // UnitOfWork.cs
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HomeTrackDBContext _context;
        public UnitOfWork(HomeTrackDBContext db)
        {
            _context = db;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }

}
