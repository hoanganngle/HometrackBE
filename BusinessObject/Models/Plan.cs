using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Plan
    {
        public Guid PlanId { get; set; }
        public string Code { get; set; } = default!;   // "FREE", "VIP"
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<PlanPrice> Prices { get; set; } = new List<PlanPrice>();
    }
}
