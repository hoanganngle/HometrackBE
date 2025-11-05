using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Order
{
    //tạo đơn hàng
    public class CreateOrderRequest
    {
        public Guid PlanPriceId { get; set; }
    }
}
