using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Chat
{
    public class GeminiOptions
    {
        public string? ApiKey { get; set; }  
        public string Model { get; set; }
        public string Endpoint { get; set; }
    }
}
