using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Plan
{
    //set giá gói, các thuộc tính của gói 
    public class PlanPriceDto
    {
        public Guid PlanPriceId { get; set; }
        public Guid PlanId { get; set; }
        public string? PlanName { get; set; }   
        public string Period { get; set; } = default!;
        public int DurationInDays { get; set; }
        public long AmountVnd { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? CreateAt { get; set; }
        [JsonIgnore]
        public DateTimeOffset? UpdateAt { get; set; }
    }
}
