using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentTransaction?> GetByProviderTxnIdAsync(string providerTxnId);
        Task AddAsync(PaymentTransaction txn);
        Task UpdateAsync(PaymentTransaction txn);

        Task<IEnumerable<PaymentTransaction>> GetAllAsync();
        Task<PaymentTransaction?> GetByIdAsync(Guid id);
        Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(Guid userId);

        Task<PaymentTransaction?> GetLatestByOrderIdAsync(Guid orderId);
        Task<List<PaymentTransaction>> GetAllByOrderIdAsync(Guid orderId);

        Task<IEnumerable<PaymentTransaction>> GetPaymentTransactionByUserIdAsync(Guid userId);
    }
}
