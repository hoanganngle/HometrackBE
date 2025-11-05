using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Auth
{
    public class UserResponse
    {
        public Guid UserId { get; set; }                 // bắt buộc
        public string Username { get; set; } = default!; // bắt buộc
        public string Email { get; set; } = default!;    // bắt buộc
        public long RoleId { get; set; }                 // bắt buộc

        // optional của user, add sau khi tạo account
        public string? PictureProfile { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public bool? Status { get; set; }
        public bool? IsPremium { get; set; }
        public bool? IsEmailVerified { get; set; }
        public DateTime? OtpGeneratedAt { get; set; }
    }
}
