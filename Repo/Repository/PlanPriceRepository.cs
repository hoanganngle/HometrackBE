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
    public class PlanPriceRepository : IPlanPriceRepository
    {
        private readonly HomeTrackDBContext _db;
        public PlanPriceRepository(HomeTrackDBContext db) => _db = db;

        public Task<PlanPrice?> GetByIdAsync(Guid id)
        {
            return _db.PlanPrices
                      .SingleOrDefaultAsync(x => x.PlanPriceId == id);
        }

        public Task<PlanPrice?> GetActiveByIdWithPlanAsync(Guid planPriceId)
        {
            return _db.PlanPrices
                      .Include(x => x.Plan) 
                      .SingleOrDefaultAsync(x => x.PlanPriceId == planPriceId
                                              && x.IsActive == true
                                              && (x.Plan == null || x.Plan.IsActive == true));
        }

        public Task<PlanPrice?> GetLatestByPlanAsync(Guid planId)
        {
            return _db.PlanPrices
                      .Include(x => x.Plan) 
                      .Where(x => x.PlanId == planId && x.IsActive == true)
                      .OrderByDescending(x => x.PlanPriceId)
                      .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PlanPrice>> GetAllAsync()
        {
            return await _db.PlanPrices
                .Include(p => p.Plan)
                .ToListAsync();
        }

        public async Task AddAsync(PlanPrice planPrice)
        {
            await _db.PlanPrices.AddAsync(planPrice);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(PlanPrice planPrice)
        {
            _db.PlanPrices.Update(planPrice);
            await _db.SaveChangesAsync();
        }
    }
}
