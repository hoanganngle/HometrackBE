using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    [Table("SubItem")]
    public class SubItem
    {
        public Guid SubItemId { get; set; }

        // GIỮ cặp này để FK tới RoomItemInRooms
        public Guid RoomId { get; set; }
        public Guid RoomItemId { get; set; }
        [JsonIgnore]
        public RoomItemInRoom Placement { get; set; }

        public string? SubItemType { get; set; }
        public string Name { get; set; }
        public string ImageUri { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? color { get; set; }
        public string? description { get; set; }
    }


}
