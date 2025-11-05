using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Payment
{
    //kiểm tra tình trạng của payment và trả về kết quả PayOSOptions
    public class StatusPayment
    {
        public string code { get; set; }

        public Guid? paymentId { get; set; }

        public string url { get; set; }

        public data Data { get; set; }

    }

    public class data
    {
        public string status { get; set; }

        public decimal amount { get; set; }
    }
}
