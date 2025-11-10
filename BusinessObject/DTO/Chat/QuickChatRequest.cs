using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Chat
{
    public sealed class QuickChatRequest
    {
        [Required, MinLength(1), MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        [MaxLength(1000)]
        [JsonIgnore]
        public string? SystemPrompt { get; set; }

        [JsonIgnore]
        public Guid? SessionId { get; set; }
    }
}
