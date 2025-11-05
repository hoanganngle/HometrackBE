using BusinessObject.DTO.Payment;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IPaymentService
    {
        Task<CreatePaymentResult> SendPaymentLink(Guid accountId, CreatePaymentRequest request);

        Task<StatusPayment> ConfirmPayment(string queryString, QueryRequest requestquery);

        Task<IEnumerable<PaymentTransactionDto>> GetAllAsync();
        Task<PaymentTransactionDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PaymentTransactionDto>> GetByUserIdAsync(Guid userId);
        Task<PaymentStatusDto?> GetStatusByOrderIdAsync(Guid orderId);

        Task<IEnumerable<PaymentTransactionDto>> GetPaymentTransactionByUserIdAsync(Guid userId);
    }
}
