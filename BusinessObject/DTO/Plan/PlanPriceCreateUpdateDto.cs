using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Plan
{
    public class PlanPriceCreateUpdateDto
    {
        public Guid PlanId { get; set; }
        public BillingPeriod Period { get; set; }   // Monthly / Yearly (Enum)
        public int DurationInDays { get; set; }
        public long AmountVnd { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
