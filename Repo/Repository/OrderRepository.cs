using BusinessObject.DTO.Order;
using BusinessObject.Enums;
using BusinessObject.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repo.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repo.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly HomeTrackDBContext _context;
        public OrderRepository(HomeTrackDBContext db) => _context = db;
        public async Task<Order?> SingleOrDefaultAsync(Expression<Func<Order, bool>> predicate)
        {
            return await _context.Orders.SingleOrDefaultAsync(predicate);
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.PlanPrice)
                    .ThenInclude(pp => pp.Plan)              
                .Include(o => o.Subscription)                
                .Include(o => o.Payments)                    
                    .ThenInclude(t => t.Refund)              
                .Include(o => o.Payments)
                    .ThenInclude(t => t.Invoice)             
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }
        public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .Include(o => o.PlanPrice).ThenInclude(pp => pp.Plan)
                .Include(o => o.Subscription)
                .Include(o => o.Payments).ThenInclude(t => t.Refund)
                .Include(o => o.Payments).ThenInclude(t => t.Invoice)
                .OrderByDescending(o => o.CreatedAt) // tùy bạn sắp xếp
                .ToListAsync();
        }



        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.PlanPrice)
                    .ThenInclude(od => od.Plan)
                .Include(o => o.Subscription)
                .Include(o => o.Payments)
                    .ThenInclude(rp => rp.Refund)
                .Include(o => o.Payments)
                        .ThenInclude(aa => aa.Invoice) 
                .ToListAsync();
        }

        public async Task<Order?> GetByOrderCodeAsync(long orderCode)
        {
            return await _context.Orders
                .AsNoTracking()
                .AsSplitQuery()
                .Include(o => o.PlanPrice).ThenInclude(pp => pp.Plan)
                .Include(o => o.Subscription)
                .Include(o => o.Payments).ThenInclude(t => t.Refund)
                .Include(o => o.Payments).ThenInclude(t => t.Invoice)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }
        public Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }
        public async Task AddAsync(Order entity) => await _context.Orders.AddAsync(entity);

        public Task<Order?> FindRecentDuplicateAsync(Guid userId, Guid planPriceId, long amountVnd, TimeSpan window)
        {
            var since = DateTime.UtcNow - window;
            return _context.Orders
                .Where(o => o.UserId == userId
                         && o.PlanPriceId == planPriceId
                         && o.AmountVnd == amountVnd
                         && o.Status == OrderStatus.Pending
                         && o.CreatedAt >= since)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int> NextSequenceForTodayAsync()
        {
            var today = DateTime.UtcNow.Date;
            var count = await _context.Orders.CountAsync(o => o.CreatedAt >= today && o.CreatedAt < today.AddDays(1));
            return count + 1;
        }
    }
}
