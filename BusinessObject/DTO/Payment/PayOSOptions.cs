using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Payment
{
    public sealed class PayOSOptions
    {
        //trả về kết quả thành công/thất bại sau khi xử lí bằng PayOS
        public string ReturnUrl { get; set; } = default!;
        public string ReturnUrlFail { get; set; } = default!;
    }

}
