using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Chat
{
    public sealed class QuickChatAudit
    {
        public string? UserId { get; set; }
        public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
        public string Provider { get; set; } = "Gemini";
        public int LatencyMs { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
