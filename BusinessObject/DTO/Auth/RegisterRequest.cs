using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Auth
{
    public class RegisterRequest
    {
        ///kiểm tra ràng buộc dữ liệu
        [Required, StringLength(255)]
        public string Username { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        [Required]
        public long RoleId { get; set; }

        // optional
        public string? PictureProfile { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        /*public bool? Status { get; set; }
        public bool? IsPremium { get; set; }
        public bool? IsEmailVerified { get; set; }
        public DateTime? OtpGeneratedAt { get; set; }*/
    }
}
