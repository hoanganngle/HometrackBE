using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Order
{
    //trả về kết quả tạo đơn hàng
    public class CreateOrderResponse
    {
        public Guid OrderId { get; set; }
        public long OrderCode { get; set; }
        public Guid PlanPriceId { get; set; }
        public long AmountVnd { get; set; }
        public int DurationInDays { get; set; }
        public OrderStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}
