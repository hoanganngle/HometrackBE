using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task AddAsync(User user);
        Task SaveChangesAsync();
        Task<User> GetUserByEmailAsync(string email);
        Task<OTPEmail> GetOTPEmailByEmail(string email);
        Task<OTPEmail> AddOTPEmail(OTPEmail otpemail);
        Task<User> GetUserByUserID(Guid? userId);
        Task SetPremiumOffAsync(IEnumerable<Guid> userIds);
    }
}
