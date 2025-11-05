using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Role
    {
        [Key]
        public long RoleId { get; set; }

        [Required]
        public string RoleName { get; set; }

        public ICollection<User> Users { get; set; }

    }
}
