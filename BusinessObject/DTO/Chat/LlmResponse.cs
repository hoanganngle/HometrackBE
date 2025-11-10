using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Chat
{
    public class LlmResponse
    {
        public string Message { get; set; }
        public List<LlmToolCall> ToolCalls { get; set; } = new List<LlmToolCall>();
    }
}
