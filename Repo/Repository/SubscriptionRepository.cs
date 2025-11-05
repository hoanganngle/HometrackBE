using BusinessObject.Enums;
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
    // SubscriptionRepository.cs
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly HomeTrackDBContext _db;
        public SubscriptionRepository(HomeTrackDBContext db) => _db = db;

        public async Task AddAsync(Subscription entity)
        {
            await _db.Subscriptions.AddAsync(entity);
        }

        public Task<Subscription?> GetByIdAsync(Guid id)
        {
            return _db.Subscriptions.SingleOrDefaultAsync(s => s.SubscriptionId == id);
        }

        public async Task<Subscription?> GetCurrentByUserAsync(Guid userId)
        {
            var now = DateTime.UtcNow;

            var active = await _db.Subscriptions
                .Where(s => s.UserId == userId && s.CurrentPeriodEnd != null && s.CurrentPeriodEnd > now)
                .OrderByDescending(s => s.CurrentPeriodEnd)
                .FirstOrDefaultAsync();

            if (active != null) return active;

            var recentExpired = await _db.Subscriptions
                .Where(s => s.UserId == userId && s.CurrentPeriodEnd != null && s.CurrentPeriodEnd <= now)
                .OrderByDescending(s => s.CurrentPeriodEnd)
                .FirstOrDefaultAsync();

            return recentExpired;
        }

        public Task UpdateAsync(Subscription entity)
        {
            _db.Subscriptions.Update(entity);
            return Task.CompletedTask;
        }

        public async Task<List<Subscription>> GetSubscriptionsNeedingDeactivateAsync()
        {
            var qPlanDur = _db.PlanPrices
                .Where(pp => pp.IsActive == null || pp.IsActive == true)
                .GroupBy(pp => pp.PlanId)
                .Select(g => new { PlanId = g.Key, DurationInDays = g.Max(x => x.DurationInDays) });

            return await _db.Subscriptions
                .Join(qPlanDur, s => s.PlanId, pd => pd.PlanId, (s, pd) => new { s, pd })
                .Where(x => x.s.CurrentPeriodStart != null && x.s.CurrentPeriodEnd != null)
                .Where(x => EF.Functions.DateDiffDay(
                                x.s.CurrentPeriodStart!.Value,
                                x.s.CurrentPeriodEnd!.Value) < x.pd.DurationInDays)
                .Where(x => x.s.Status == SubscriptionStatus.Active)
                .Select(x => x.s)
                .ToListAsync();
        }

        public async Task BulkUpdateAsync(IEnumerable<Subscription> subs)
        {
            _db.Subscriptions.UpdateRange(subs);
            await _db.SaveChangesAsync();
        }

    }

}
