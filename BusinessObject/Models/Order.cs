using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }

        public long OrderCode { get; set; } = default!;

        public Guid UserId { get; set; }

        public Guid? SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = default!;

        public Guid PlanPriceId { get; set; }
        public PlanPrice PlanPrice { get; set; } = default!;

        public long AmountVnd { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? PaidAt { get; set; }

        public ICollection<PaymentTransaction> Payments { get; set; } = new List<PaymentTransaction>();
    }
}
