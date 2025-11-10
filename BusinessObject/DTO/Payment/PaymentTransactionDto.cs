using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Payment
{
    public class PaymentTransactionDto
    {
        public Guid PaymentTransactionId { get; set; }

        public Guid OrderId { get; set; }
        public string? OrderCode { get; set; }

        public PaymentProvider Provider { get; set; }
        public PaymentStatus Status { get; set; }

        public long AmountVnd { get; set; }
        public string ProviderTransactionId { get; set; } = default!;
        public string? Signature { get; set; }

        public string? RequestPayload { get; set; }
        public string? ResponsePayload { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? SucceededAt { get; set; }
    }

    public record PaymentStatusDto(
    Guid OrderId,
    string Status,     
    long AmountVnd,
    string? ProviderTransactionId,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? SucceededAt,
    string? RawResponse           // tuỳ, có thể bỏ nếu không cần
    );
}
