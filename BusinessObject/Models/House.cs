using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    [Table("House")]
    public class House
    {
        [Key]
        public Guid HouseId { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        public User User { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public ICollection<Floor> Floors { get; set; } = new List<Floor>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? Image {  get; set; }
    }
}
