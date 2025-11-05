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
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly HomeTrackDBContext _context;
        public InvoiceRepository(HomeTrackDBContext db)
        {
            _context = db;
        }

        public async Task<bool> ExistsByTransactionIdAsync(Guid paymentTransactionId)
        {
            var exists = await _context.Invoices
                .AsNoTracking()
                .AnyAsync(i => i.PaymentTransactionId == paymentTransactionId);
            return exists;
        }

        public async Task AddAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
        }
    }

}
