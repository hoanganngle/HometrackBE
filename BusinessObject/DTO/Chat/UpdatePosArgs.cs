using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Chat
{
   public class UpdatePosArgs
    {
        public string roomId { get; set; } = "";
        public string roomItemId { get; set; } = "";
        public double x { get; set; }
        public double y { get; set; }
    }
}
