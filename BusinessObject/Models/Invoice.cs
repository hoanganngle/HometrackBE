using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Invoice
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = default!; // unique
        public Guid UserId { get; set; }

        public Guid PaymentTransactionId { get; set; }
        public PaymentTransaction PaymentTransaction { get; set; } = default!;

        public long SubtotalVnd { get; set; }
        public long TaxVnd { get; set; }
        public long TotalVnd { get; set; }
        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? InvoiceJsonSnapshot { get; set; }
        public string? PdfPath { get; set; }
    }
}
