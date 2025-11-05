using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IOrderRepository 
    {
        Task<Order?> SingleOrDefaultAsync(Expression<Func<Order, bool>> predicate);
        Task<Order> GetOrderByIdAsync(Guid orderId);
        Task<List<Order>> GetOrdersByUserIdAsync(Guid userId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order?> GetByOrderCodeAsync(long orderCode);
        Task UpdateAsync(Order order);
        Task AddAsync(Order entity);
        Task<Order?> FindRecentDuplicateAsync(Guid userId, Guid planPriceId, long amountVnd, TimeSpan window);
        Task<int> NextSequenceForTodayAsync();

    }
}
