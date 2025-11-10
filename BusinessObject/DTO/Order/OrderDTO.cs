using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Order
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public long OrderCode { get; set; }
        public Guid UserId { get; set; }
        public Guid? SubscriptionId { get; set; }
        public Guid PlanPriceId { get; set; }
        public long AmountVnd { get; set; }
        public OrderStatus Status { get; set; }
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
    }
}
