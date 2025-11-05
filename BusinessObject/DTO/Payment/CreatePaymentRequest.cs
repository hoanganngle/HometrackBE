using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Payment
{
    public class CreatePaymentRequest
    {
        public Guid? OrderId { get; set; }
        [JsonIgnore]
        public string? Description { get; set; } = "";

        // Số tiền thanh toán (đơn vị: VND)
    }
}
