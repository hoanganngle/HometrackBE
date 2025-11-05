using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class PaymentTransaction
    {
        public Guid PaymentTransactionId { get; set; }

        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;

        public PaymentProvider Provider { get; set; } = PaymentProvider.PayOS;
        public PaymentStatus Status { get; set; } = PaymentStatus.Paid;

        public long AmountVnd { get; set; }
        public string ProviderTransactionId { get; set; } = default!; // unique từ PayOS
        public string? Signature { get; set; }

        public string? RequestPayload { get; set; }   // JSON
        public string? ResponsePayload { get; set; }  // JSON

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? SucceededAt { get; set; }

        public Refund? Refund { get; set; }
        public Invoice? Invoice { get; set; }

    }
}
