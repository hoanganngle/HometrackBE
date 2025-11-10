using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Order
{
    public class CancelSubscriptionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public CancelSubscriptionResult() { }
        public CancelSubscriptionResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
