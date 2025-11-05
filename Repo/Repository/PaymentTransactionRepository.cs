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
    // PaymentTransactionRepository.cs
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly HomeTrackDBContext _context;
        public PaymentTransactionRepository(HomeTrackDBContext db)
        {
            _context = db;
        }

        public async Task<PaymentTransaction?> GetByProviderTxnIdAsync(string providerTxnId)
        {
            var txn = await _context.PaymentTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ProviderTransactionId == providerTxnId);
            return txn;
        }

        public async Task AddAsync(PaymentTransaction txn)
        {
            await _context.PaymentTransactions.AddAsync(txn);
        }

        public Task UpdateAsync(PaymentTransaction txn)
        {
            _context.PaymentTransactions.Update(txn);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<PaymentTransaction>> GetAllAsync()
        {
            return await _context.PaymentTransactions
                .Include(p => p.Order)
                .Include(p => p.Refund)
                .Include(p => p.Invoice)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PaymentTransaction?> GetByIdAsync(Guid id)
        {
            return await _context.PaymentTransactions
                .Include(p => p.Order)
                .Include(p => p.Refund)
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.PaymentTransactionId == id);
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(Guid userId)
        {
            return await _context.PaymentTransactions
                .Include(p => p.Order)
                .Include(p => p.Refund)
                .Include(p => p.Invoice)
                .Where(p => p.Order.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PaymentTransaction?> GetLatestByOrderIdAsync(Guid orderId)
        {
            return await _context.PaymentTransactions
                      .AsNoTracking()
                      .Where(x => x.OrderId == orderId)
                      .OrderByDescending(x => x.SucceededAt ?? x.CreatedAt)
                      .FirstOrDefaultAsync(); // <-- trả về 1 bản ghi (PaymentTransaction?)
        }

        public async Task<List<PaymentTransaction>> GetAllByOrderIdAsync(Guid orderId)
        {
            return await _context.Set<PaymentTransaction>()
                      .AsNoTracking()
                      .Where(x => x.OrderId == orderId)
                      .OrderByDescending(x => x.SucceededAt ?? x.CreatedAt)
                      .ToListAsync();
        }
        public async Task<IEnumerable<PaymentTransaction>> GetPaymentTransactionByUserIdAsync(Guid userId)
        {
            return await _context.PaymentTransactions
                .Include(pt => pt.Order)
                .Where(pt => pt.Order.UserId == userId)
                .ToListAsync();
        }
    }

}
