using BusinessObject.Models;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IPlanPriceRepository 
    {
        Task<PlanPrice?> GetActiveByIdWithPlanAsync(Guid planPriceId);
        Task<PlanPrice?> GetByIdAsync(Guid id);

        Task<PlanPrice?> GetLatestByPlanAsync(Guid planId);

        Task<IEnumerable<PlanPrice>> GetAllAsync();
        Task AddAsync(PlanPrice planPrice);
        Task UpdateAsync(PlanPrice planPrice);
    }
}
