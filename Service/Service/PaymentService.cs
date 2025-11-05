using BusinessObject.DTO.Email;
using BusinessObject.DTO.Payment;
using BusinessObject.Enums;
using BusinessObject.Models;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Newtonsoft.Json;
using Repo.IRepository;
using Service.Exceptions;
using Service.IService;
using System.Security.Cryptography;
using System.Text;

namespace Service.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly IPaymentTransactionRepository _txnRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly IWebhookLogRepository _webhookLogRepo;
        private readonly IUnitOfWork _uow;
        private readonly IPlanPriceRepository _planPriceRepo;
        private readonly IPaymentTransactionRepository _paymentRepo;
        public PaymentService(
            IOrderRepository orderRepository, 
            IUserRepository userRepository, 
            IConfiguration configuration, 
            HttpClient client,
            IPaymentTransactionRepository txnRepo,
            IInvoiceRepository invoiceRepo,
            ISubscriptionRepository subscriptionRepo,
            IWebhookLogRepository webhookLogRepo,
            IUnitOfWork uow,
            IHttpClientFactory httpClientFactory,
            IPlanPriceRepository planPriceRepo,
            IPaymentTransactionRepository transactionRepository
            )
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _client = client;
            _txnRepo = txnRepo;
            _invoiceRepo = invoiceRepo;
            _subscriptionRepo = subscriptionRepo;
            _webhookLogRepo = webhookLogRepo;
            _uow = uow;
            _planPriceRepo = planPriceRepo;
            _paymentRepo = transactionRepository;
        }
        public async Task<CreatePaymentResult> SendPaymentLink(Guid accountId, CreatePaymentRequest request)
        {
            try
            {
                if (request.OrderId == null) throw new ArgumentException("OrderId is required.");

                var order = await _orderRepository.SingleOrDefaultAsync(p => p.OrderId == request.OrderId);
                if (order == null)
                    throw new Exception("Không tìm thấy đơn hàng.");

                int amountToPay = checked((int)order.AmountVnd);
                if (amountToPay < 5000)
                    throw new BusinessException("Số tiền thanh toán tối thiểu là 5.000đ.", 400);

                long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                //long orderCode = order.OrderCode;


                var user = await _userRepository.GetUserByUserID(accountId);
                if (user == null) throw new Exception("Tài khoản không hợp lệ.");

                string description = request.Description;
                string? clientId = _configuration["PayOS:ClientId"];
                var apikey = _configuration["PayOS:APIKey"];
                var checksumkey = _configuration["PayOS:ChecksumKey"];
                var returnurlfail = _configuration["PayOS:ReturnUrlFail"];

                string returnUrl = $"https://hometrack.mlhr.org/api/Payment/paymentconfirm" +
                //string returnUrl = $"https://localhost:7227/api/Payment/paymentconfirm" +
                    $"?orderCode={orderCode}" +
                    $"&orderId={request.OrderId}";

                var signatureData = new Dictionary<string, object>
                {
                    { "amount", amountToPay },
                    { "cancelUrl", returnurlfail },
                    { "description", description },
                    { "expiredAt", DateTimeOffset.Now.ToUnixTimeSeconds() },
                    { "orderCode", orderCode },
                    { "returnUrl", returnUrl }
                    };

                var sortedSignatureData = new SortedDictionary<string, object>(signatureData);
                var dataForSignature = string.Join("&", sortedSignatureData.Select(p => $"{p.Key}={p.Value}"));
                var signature = ComputeHmacSha256(dataForSignature, checksumkey);

                PayOS pos = new PayOS(clientId, apikey, checksumkey);

                var paymentData = new PaymentData(
                    orderCode: orderCode,
                    amount: amountToPay,
                    description: description,
                    items: new List<ItemData> { new ItemData(user.Username, 1, amountToPay) },
                    cancelUrl: returnurlfail,
                    returnUrl: returnUrl,
                    signature: signature,
                    buyerName: user.Username,
                    expiredAt: (int)DateTimeOffset.Now.AddMinutes(10).ToUnixTimeSeconds()
                );

                var createPaymentResult = await pos.createPaymentLink(paymentData);
                return createPaymentResult;
            }
            catch (BusinessException ex)
            {
                Console.WriteLine($"Lỗi tạo liên kết thanh toán: {ex.Message}");
                throw;
            }
        }
        private string ComputeHmacSha256(string data, string checksumKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public async Task<StatusPayment> ConfirmPayment(string queryString, QueryRequest req)
        {
            if (req == null || req.orderCode <= 0)
                throw new ArgumentException("orderCode không hợp lệ.");

            // 1) Lấy Order theo orderCode
            var order = await _orderRepository.GetOrderByIdAsync(req.OrderId);
            if (order == null) throw new Exception("Không tìm thấy đơn hàng.");

            // 2) Gọi PayOS check trạng thái
            var url = $"https://api-merchant.payos.vn/v2/payment-requests/{req.orderCode}";
            var http = new HttpRequestMessage(HttpMethod.Get, url);
            http.Headers.Add("x-client-id", _configuration["PayOS:ClientId"]);
            http.Headers.Add("x-api-key", _configuration["PayOS:APIKey"]);

            var resp = await _client.SendAsync(http);
            if (!resp.IsSuccessStatusCode) throw new Exception("Không gửi được yêu cầu tới PayOS.");

            var json = await resp.Content.ReadAsStringAsync();
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
            var status = (string?)obj["data"]?["status"];            // PAID | PENDING | ...
            var providerTxnId = (string?)obj["data"]?["paymentLinkId"];     // id giao dịch bên PayOS
            var amountFromPay = (long?)obj["data"]?["amount"] ?? 0;

            // 3) Idempotency theo ProviderTransactionId
            var existedTxn = string.IsNullOrWhiteSpace(providerTxnId)
                ? null
                : await _txnRepo.GetByProviderTxnIdAsync(providerTxnId);

            var now = DateTimeOffset.UtcNow;
            var txn = existedTxn ?? new PaymentTransaction
            {
                OrderId = order.OrderId,
                Provider = PaymentProvider.PayOS,
                CreatedAt = now
            };

            txn.ProviderTransactionId = !string.IsNullOrWhiteSpace(providerTxnId)
                ? providerTxnId
                : order.OrderCode.ToString();

            txn.AmountVnd = order.AmountVnd;
            txn.Status = PaymentStatus.Paid;
            txn.SucceededAt = now;

            // log trace
            txn.RequestPayload = txn.RequestPayload ?? null; 
            txn.ResponsePayload = json;
            txn.Signature = null;

            if (existedTxn == null)
                await _txnRepo.AddAsync(txn);
            else
                await _txnRepo.UpdateAsync(txn);

            await _uow.SaveChangesAsync();

            order.Status = OrderStatus.Paid;
            order.PaidAt = now;
            await _orderRepository.UpdateAsync(order);

            var hasInvoice = await _invoiceRepo.ExistsByTransactionIdAsync(txn.PaymentTransactionId);
            if (!hasInvoice)
            {
                var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{order.OrderCode}";
                var snapshotJson = JsonConvert.SerializeObject(new
                {
                    invoiceNumber,
                    userId = order.UserId,
                    orderCode = order.OrderCode,
                    amount = order.AmountVnd,
                    provider = txn.Provider.ToString(),
                    providerTransactionId = txn.ProviderTransactionId,
                    paidAt = order.PaidAt,
                    createdAt = now
                });

                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    UserId = order.UserId,
                    PaymentTransactionId = txn.PaymentTransactionId,   
                    SubtotalVnd = order.AmountVnd,
                    TaxVnd = 0,
                    TotalVnd = order.AmountVnd,
                    IssuedAt = now,
                    CreatedAt = now,
                    InvoiceJsonSnapshot = snapshotJson,
                    PdfPath = null
                };
                await _invoiceRepo.AddAsync(invoice);
            }

            var planPrice = await _planPriceRepo.GetByIdAsync(order.PlanPriceId)
                ?? throw new Exception("Không tìm thấy PlanPrice của đơn hàng.");
            var durationDays = planPrice.DurationInDays;

            if (order.SubscriptionId == null || order.SubscriptionId == Guid.Empty)
            {
                var start = now;
                var end = start.AddDays(durationDays);
                var user = await _userRepository.GetUserByUserID(order.UserId);

                var sub = new Subscription
                {
                    SubscriptionId = Guid.NewGuid(),
                    UserId = order.UserId,
                    PlanId = planPrice.PlanId,
                    Status = SubscriptionStatus.Active,
                    CancelAtPeriodEnd = false,
                    CancelledAt = null,
                    CurrentPeriodStart = start,
                    CurrentPeriodEnd = end,
                    LatestOrderCode = order.OrderCode.ToString()
                };

                await _subscriptionRepo.AddAsync(sub);
                user.IsPremium = true;
                await _uow.SaveChangesAsync();         
                order.SubscriptionId = sub.SubscriptionId;
            }
            else
            {
                var sub = await _subscriptionRepo.GetByIdAsync(order.SubscriptionId.Value)
                          ?? throw new Exception("Subscription không tồn tại.");

                var baseTime = (sub.CurrentPeriodEnd.HasValue && sub.CurrentPeriodEnd.Value > now)
                                ? sub.CurrentPeriodEnd.Value
                                : now;

                sub.Status = SubscriptionStatus.Active;
                sub.CancelAtPeriodEnd = false;
                sub.CancelledAt = null;
                sub.CurrentPeriodStart = baseTime;
                sub.CurrentPeriodEnd = baseTime.AddDays(durationDays);
                sub.LatestOrderCode = order.OrderCode.ToString();

                await _subscriptionRepo.UpdateAsync(sub);
            }

            await _orderRepository.UpdateAsync(order);
            await _uow.SaveChangesAsync();


            /*// 9) Log
            await _webhookLogRepo.AddAsync(new WebhookLog
            {
                EventType = "returnUrl.confirm",
                OrderCode = order.OrderCode.ToString(),
                ProviderTransactionId = providerTxnId,

                RawPayload = json,     
                Payload = json,        
                Verified = false,      
                VerificationError = null,

                ReceivedAt = now
            });*/

            // 10) Commit
            await _uow.SaveChangesAsync();

            return new StatusPayment
            {
                code = "00",
                Data = new data { status = "PAID", amount = order.AmountVnd }
            };
        }

        public async Task<IEnumerable<PaymentTransactionDto>> GetAllAsync()
        {
            var list = await _txnRepo.GetAllAsync();

            return list.Select(p => new PaymentTransactionDto
            {
                PaymentTransactionId = p.PaymentTransactionId,
                OrderId = p.OrderId,
                OrderCode = p.Order?.OrderCode.ToString() ?? "N/A", 
                Provider = p.Provider,
                Status = p.Status,
                AmountVnd = p.AmountVnd,
                ProviderTransactionId = p.ProviderTransactionId,
                Signature = p.Signature,
                RequestPayload = p.RequestPayload,
                ResponsePayload = p.ResponsePayload,
                CreatedAt = p.CreatedAt,
                SucceededAt = p.SucceededAt
            }).ToList();
        }

        public async Task<PaymentTransactionDto> GetByIdAsync(Guid id)
        {
            var p = await _txnRepo.GetByIdAsync(id);
            if (p == null)
                throw new Exception($"PaymentTransaction with ID {id} not found.");

            return new PaymentTransactionDto
            {
                PaymentTransactionId = p.PaymentTransactionId,
                OrderId = p.OrderId,
                OrderCode = p.Order?.OrderCode.ToString() ?? "N/A",
                Provider = p.Provider,
                Status = p.Status,
                AmountVnd = p.AmountVnd,
                ProviderTransactionId = p.ProviderTransactionId,
                Signature = p.Signature,
                RequestPayload = p.RequestPayload,
                ResponsePayload = p.ResponsePayload,
                CreatedAt = p.CreatedAt,
                SucceededAt = p.SucceededAt
            };
        }

        public async Task<IEnumerable<PaymentTransactionDto>> GetByUserIdAsync(Guid userId)
        {
            var list = await _txnRepo.GetByUserIdAsync(userId);
            if (!list.Any())
                throw new Exception($"No transactions found for User with ID {userId}.");

            return list.Select(p => new PaymentTransactionDto
            {
                PaymentTransactionId = p.PaymentTransactionId,
                OrderId = p.OrderId,
                OrderCode = p.Order?.OrderCode.ToString() ?? "N/A",
                Provider = p.Provider,
                Status = p.Status,
                AmountVnd = p.AmountVnd,
                ProviderTransactionId = p.ProviderTransactionId,
                Signature = p.Signature,
                RequestPayload = p.RequestPayload,
                ResponsePayload = p.ResponsePayload,
                CreatedAt = p.CreatedAt,
                SucceededAt = p.SucceededAt
            }).ToList();
        }
        public async Task<PaymentStatusDto?> GetStatusByOrderIdAsync(Guid orderId)
        {
            if (orderId == Guid.Empty) return null;

            var tx = await _paymentRepo.GetLatestByOrderIdAsync(orderId);
            if (tx is null) return null;

            return new PaymentStatusDto(
                OrderId: tx.OrderId,
                Status: tx.Status.ToString(),  
                AmountVnd: tx.AmountVnd,
                ProviderTransactionId: tx.ProviderTransactionId,
                CreatedAt: tx.CreatedAt,
                SucceededAt: tx.SucceededAt,
                RawResponse: tx.ResponsePayload
            );
        }

        public async Task<IEnumerable<PaymentTransactionDto>> GetPaymentTransactionByUserIdAsync(Guid userId)
        {
            var entities = await _paymentRepo.GetPaymentTransactionByUserIdAsync(userId);
            return entities.Select(ToDto);
        }
        private PaymentTransactionDto ToDto(PaymentTransaction p)
        {
            return new PaymentTransactionDto
            {
                PaymentTransactionId = p.PaymentTransactionId,
                OrderId = p.OrderId,
                OrderCode = p.Order?.OrderCode.ToString() ?? "N/A",
                Provider = p.Provider,
                Status = p.Status,
                AmountVnd = p.AmountVnd,
                ProviderTransactionId = p.ProviderTransactionId,
                Signature = p.Signature,
                RequestPayload = p.RequestPayload,
                ResponsePayload = p.ResponsePayload,
                CreatedAt = p.CreatedAt,
                SucceededAt = p.SucceededAt
            };
        }
    }
}
