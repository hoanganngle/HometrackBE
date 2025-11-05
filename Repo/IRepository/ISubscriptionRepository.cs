using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface ISubscriptionRepository
    {
        Task AddAsync(Subscription entity);
        Task UpdateAsync(Subscription entity);
        Task<Subscription?> GetByIdAsync(Guid id);
        Task<Subscription?> GetCurrentByUserAsync(Guid userId);

        Task<List<Subscription>> GetSubscriptionsNeedingDeactivateAsync();
        Task BulkUpdateAsync(IEnumerable<Subscription> subs);
    }
}
