using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    [Table("Floor")]
    public class Floor
    {
        [Key]
        public Guid FloorId { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(House))]
        public Guid HouseId { get; set; }
        public House House { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public ICollection<Room> Rooms { get; set; } = new List<Room>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
