using BusinessObject.DTO.Order;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IOrderService
    {
        Task<OrderDTO> GetOrderByIdAsync(Guid orderId);
        Task<List<OrderDTO>> GetOrdersByUserIdAsync(Guid userId);
        
        Task<List<OrderDTO>> GetAllOrdersAsync();
        Task<OrderDTO> GetByOrderCodeAsync(long orderCode);
        Task<CreateOrderResponse> CreateUpgradeOrderAsync(Guid userId, CreateOrderRequest request);
        
    }
}
