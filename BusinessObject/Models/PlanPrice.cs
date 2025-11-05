using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class PlanPrice
    {
        public Guid PlanPriceId { get; set; }
        public Guid PlanId { get; set; }
        public Plan Plan { get; set; } = default!;

        public BillingPeriod Period { get; set; }      // Monthly/Yearly
        public int DurationInDays { get; set; }        // 30/365...
        public long AmountVnd { get; set; }            // VNĐ (integer)
        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreateAt { get; set; }
        public DateTimeOffset? UpdateAt { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
