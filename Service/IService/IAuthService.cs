using BusinessObject.DTO.Auth;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IAuthService
    {
        Task<ApiResult<UserResponse>> RegisterAsync(RegisterRequest req);
        Task<ApiResult<LoginResult>> LoginAsync(LoginRequest req);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<User> GetUserByIdAsync(Guid userId);
        Task AddAsync(Role role);

        Task<bool> SetStatusAsync(Guid userId, CancellationToken ct = default);
    }
}
