using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class OTPEmail
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? OtpKey { get; set; }
        public DateTime? CreatedTime { get; set; }
        public double? ExpireTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
