using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IInvoiceRepository
    {
        Task<bool> ExistsByTransactionIdAsync(Guid paymentTransactionId);
        Task AddAsync(Invoice invoice);
    }
}
