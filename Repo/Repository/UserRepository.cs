using BusinessObject.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repo.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly HomeTrackDBContext _context;
        public UserRepository(HomeTrackDBContext db) => _context = db;

        public Task<User?> GetByEmailAsync(string email) =>
            _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public Task<bool> EmailExistsAsync(string email) =>
            _context.Users.AnyAsync(u => u.Email == email);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();

        public Task<bool> UsernameExistsAsync(string username) =>
        _context.Users.AnyAsync(u => u.Username == username);

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<OTPEmail> GetOTPEmailByEmail(string email)
        {
            var otpEmail = await _context.OTPEmails.FirstOrDefaultAsync(a => a.Email == email);
            return otpEmail;
        }
        public async Task<OTPEmail> AddOTPEmail(OTPEmail otpemail)
        {
            _context.OTPEmails.Add(otpemail);
            await _context.SaveChangesAsync();
            return otpemail;
        }
        public async Task<User> GetUserByUserID(Guid? userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task SetPremiumOffAsync(IEnumerable<Guid> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return;

            var users = await _context.Users.Where(u => ids.Contains(u.UserId)).ToListAsync();
            foreach (var u in users) u.IsPremium = false;

            await _context.SaveChangesAsync();
        }

       
    }

}
