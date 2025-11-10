using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Chat
{
    public sealed class QuickChatResponse
    {
        public QuickChatResponse(string reply, Guid sessionId)
        {
            Reply = reply;
            SessionId = sessionId;
        }
        public string Reply { get; set; }

        public Guid SessionId { get; set; }
    }
}
