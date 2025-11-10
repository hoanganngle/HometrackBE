using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Auth
{
    public class LoginResult
    {
        public string Token { get; set; } = default!;
        public string Username { get; set; } = default!;
        public long RoleId { get; set; } = default!;
        public Guid userId { get; set; }
        public string? PictureProfile { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public bool? Status { get; set; }
        public bool? IsPremium { get; set; }
        public bool? IsEmailVerified { get; set; }
        public DateTime? OtpGeneratedAt { get; set; }
        //public UserResponse User { get; set; } = default!;
    }

}
