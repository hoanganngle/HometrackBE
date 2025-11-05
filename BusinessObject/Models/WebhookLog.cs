using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class WebhookLog
    {
        public Guid WebhookLogId { get; set; }
        public string EventType { get; set; } = default!;
        public string? OrderCode { get; set; }
        public string? ProviderTransactionId { get; set; }
        public string RawPayload { get; set; } = default!;

        public bool Verified { get; set; }
        public string? VerificationError { get; set; }
        public DateTimeOffset ReceivedAt { get; set; }

        public string? Payload { get; set; }
    }
}
