using BusinessObject.DTO.Order;
using BusinessObject.DTO.Payment;
using BusinessObject.Enums;
using BusinessObject.Models;
using Microsoft.Extensions.Options;
using Repo.IRepository;
using Repo.Repository;
using Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _uow;
        private readonly IPlanPriceRepository _planPriceRepo;
        private readonly PayOSOptions _payos;
        
        public OrderService(IOrderRepository orderRepo, 
            IPlanPriceRepository planPriceRepo, 
            IUnitOfWork uow, 
            IOptions<PayOSOptions> payos
            )
        {
            _orderRepository = orderRepo;
            _planPriceRepo = planPriceRepo;
            _uow = uow;
            _payos = payos.Value;
        }


        public async Task<OrderDTO> GetOrderByIdAsync(Guid orderId)
        {
            // Lấy entity Order theo Id
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            long orderCode = order.OrderCode;

            // Trả về DTO đúng schema mới
            return new OrderDTO
            {
                Id = order.OrderId,
                OrderCode = orderCode,
                UserId = order.UserId,
                SubscriptionId = order.SubscriptionId,
                PlanPriceId = order.PlanPriceId,
                AmountVnd = order.AmountVnd,
                Status = order.Status,          
                ReturnUrl = order.ReturnUrl,
                CancelUrl = order.CancelUrl,
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt
            };
        }
        public async Task<List<OrderDTO>> GetOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId)
                         ?? new List<Order>(); // phòng repo trả null (ToListAsync() thì không null)

            return orders.Select(order => new OrderDTO
            {
                Id = order.OrderId,
                OrderCode = order.OrderCode,
                UserId = order.UserId,
                SubscriptionId = order.SubscriptionId,
                PlanPriceId = order.PlanPriceId,
                AmountVnd = order.AmountVnd,
                Status = order.Status,
                ReturnUrl = order.ReturnUrl,
                CancelUrl = order.CancelUrl,
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt
            }).ToList(); // nếu orders rỗng -> trả [].
        }




        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();

            return orders.OrderByDescending(re => re.CreatedAt).Select(o => new OrderDTO
            {
                Id = o.OrderId,
                OrderCode = o.OrderCode,
                UserId = o.UserId,
                SubscriptionId = o.SubscriptionId,
                PlanPriceId = o.PlanPriceId,
                AmountVnd = o.AmountVnd,
                Status = o.Status,
                ReturnUrl = o.ReturnUrl,
                CancelUrl = o.CancelUrl,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt

            }).ToList();
        }

        public async Task<OrderDTO> GetByOrderCodeAsync(long orderCode)
        {
            var order = await _orderRepository.GetByOrderCodeAsync(orderCode);
            if (order == null)
                throw new KeyNotFoundException($"Order with OrderCode {orderCode} not found.");

            return new OrderDTO
            {
                Id = order.OrderId,
                OrderCode = order.OrderCode,
                UserId = order.UserId,
                SubscriptionId = order.SubscriptionId,
                PlanPriceId = order.PlanPriceId,
                AmountVnd = order.AmountVnd,
                Status = order.Status,
                ReturnUrl = order.ReturnUrl,
                CancelUrl = order.CancelUrl,
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt
            };
        }

        public async Task<CreateOrderResponse> CreateUpgradeOrderAsync(Guid userId, CreateOrderRequest request)
        {
            // 1) Validate PlanPrice (và Plan) đang active
            var pp = await _planPriceRepo.GetActiveByIdWithPlanAsync(request.PlanPriceId);
            if (pp == null) throw new InvalidOperationException("PlanPrice không hợp lệ hoặc đã ngừng bán.");

            var amountVnd = pp.AmountVnd;
            var duration = pp.DurationInDays;

            // 2) Idempotency nhẹ: nếu trong 10' có Pending trùng -> trả về đơn cũ
            var dup = await _orderRepository.FindRecentDuplicateAsync(userId, pp.PlanPriceId, amountVnd, TimeSpan.FromMinutes(10));
            if (dup != null)
            {
                return new CreateOrderResponse
                {
                    OrderId = dup.OrderId,
                    OrderCode = dup.OrderCode,
                    PlanPriceId = dup.PlanPriceId,
                    AmountVnd = dup.AmountVnd,
                    DurationInDays = duration,
                    Status = dup.Status,
                    CreatedAt = dup.CreatedAt,
                    ReturnUrl = dup.ReturnUrl,
                    CancelUrl = dup.CancelUrl
                };
            }

            var seq = await _orderRepository.NextSequenceForTodayAsync(); // đếm theo ngày
            var orderCode = BuildOrderCode9DigitsLong(seq, DateTime.UtcNow);

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderCode = orderCode,
                UserId = userId,
                SubscriptionId = null,           
                PlanPriceId = pp.PlanPriceId,
                AmountVnd = amountVnd,
                Status = OrderStatus.Pending,
                ReturnUrl = _payos.ReturnUrl,
                CancelUrl = _payos.ReturnUrlFail,
                CreatedAt = DateTime.UtcNow,
                PaidAt = null
            };

            await _orderRepository.AddAsync(order);
            await _uow.SaveChangesAsync();

            return new CreateOrderResponse
            {
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                PlanPriceId = order.PlanPriceId,
                AmountVnd = order.AmountVnd,
                DurationInDays = duration,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                ReturnUrl = order.ReturnUrl,
                CancelUrl = order.CancelUrl
            };
        }
        private static long BuildOrderCode9DigitsLong(int seq, DateTime utcNow)
        {
            int yy = utcNow.Year % 100;          
            int ddd = utcNow.DayOfYear;            
            int s4 = Math.Clamp(seq, 1, 9999);    
            return yy * 1_000_000L + ddd * 10_000L + s4; 
        }

        

    }

}

