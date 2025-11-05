using BusinessObject.DTO.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface ISubscriptionService
    {
        Task<CancelSubscriptionResult> CancelByUserAsync(Guid userId);
        Task<int> DeactivateInvalidSubscriptionsAsync();
    }
}
