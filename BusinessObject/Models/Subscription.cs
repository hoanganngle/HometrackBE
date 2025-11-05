using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Subscription
    {
        public Guid SubscriptionId { get; set; }
        public Guid UserId { get; set; }               // FK sang Users của bạn
        public Guid PlanId { get; set; }
        public Plan Plan { get; set; } = default!;

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Inactive;
        public DateTimeOffset? CurrentPeriodStart { get; set; }
        public DateTimeOffset? CurrentPeriodEnd { get; set; }
        public bool CancelAtPeriodEnd { get; set; } = false;
        public DateTimeOffset? CancelledAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? LatestOrderCode { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
