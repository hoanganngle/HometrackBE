using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Exceptions
{
    //trả về exception, khi có lỗi trong quá trình xử lý nghiệp vụ
    public class BusinessException : System.Exception
    {
        public int StatusCode { get; }

        public BusinessException(string message, int statusCode = 400)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public BusinessException(string message, System.Exception? inner, int statusCode = 400)
            : base(message, inner)
        {
            StatusCode = statusCode;
        }
    }
}
