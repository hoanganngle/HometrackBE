using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ChatSessionId { get; set; }
        public ChatSession ChatSession { get; set; }

        public string UserId { get; set; }   // ai gửi
        public string Role { get; set; }     // "user" | "assistant" | "system"
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
