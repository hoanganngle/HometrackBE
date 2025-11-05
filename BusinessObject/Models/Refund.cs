using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Refund
    {
        public Guid RefundId { get; set; }
        public Guid PaymentTransactionId { get; set; }
        public PaymentTransaction PaymentTransaction { get; set; } = default!;

        public long AmountVnd { get; set; }
        public string Reason { get; set; } = default!;
        public string ProviderRefundId { get; set; } = default!;
        public DateTimeOffset RefundedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
