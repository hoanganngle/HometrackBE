using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Chat
{
    public class LlmToolCall
    {
        public string Name { get; set; }
        public JsonElement Arguments { get; set; }

        public LlmToolCall() { } // để dùng object initializer
        public LlmToolCall(string name, JsonElement arguments) // optional: nếu vẫn muốn gọi ctor 2 tham số
        {
            Name = name ?? "";
            Arguments = arguments;
        }
    }
}
