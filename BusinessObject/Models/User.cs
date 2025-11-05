using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid(); // fix GUID

        [Required]
        [StringLength(255)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [ForeignKey(nameof(Role))]
        public long RoleId { get; set; } // 1: admin, 2: user
        public Role Role { get; set; }
        public string? PictureProfile { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public bool? Status { get; set; }
        public bool? IsPremium { get; set; }
        public bool? IsEmailVerified { get; set; }
        public DateTime? OtpGeneratedAt { get; set; }
    }
}
