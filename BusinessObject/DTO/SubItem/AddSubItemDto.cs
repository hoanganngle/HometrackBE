using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.SubItem
{
    public class AddSubItemDto
    {
        [Required]
        public Guid RoomItemId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public string? ImageUri { get; set; }
        public int SortOrder { get; set; } = 0;

        public string? corlor {  get; set; }
        public string? description { get; set; }
    }
}
