using BusinessObject.DTO.Auth;
using BusinessObject.DTO.Email;
using BusinessObject.Models;
using Org.BouncyCastle.Ocsp;
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
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _usersRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly JwtService _jwtService;
        private readonly IEmailService _mailService;
        public AuthService(IUserRepository users, IRoleRepository roleRepository, JwtService jwtService, IEmailService emailService)
        {
            _usersRepository = users;
            _roleRepository = roleRepository;
            _jwtService = jwtService;   
            _mailService = emailService;
        }

        public async Task<ApiResult<UserResponse>> RegisterAsync(RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username))
                return ApiResult<UserResponse>.Fail("Username là bắt buộc.");
            if (string.IsNullOrWhiteSpace(req.Email))
                return ApiResult<UserResponse>.Fail("Email là bắt buộc.");
            /*if (string.IsNullOrWhiteSpace(req.Password))
                return ApiResult<UserResponse>.Fail("Password là bắt buộc.");*/

            if (await _usersRepository.EmailExistsAsync(req.Email.Trim()))
                return ApiResult<UserResponse>.Fail("Email đã tồn tại.");
            if (await _usersRepository.UsernameExistsAsync(req.Username.Trim()))
                return ApiResult<UserResponse>.Fail("Username đã tồn tại.");
            var user = new User
            {
                Username = req.Username.Trim(),
                Email = req.Email.Trim(),
                Password = "As@123456",             
                RoleId = req.RoleId,                         
                PictureProfile = req.PictureProfile,
                DateOfBirth = req.DateOfBirth ?? default,
                Phone = req.Phone,
                Status = false,       
                IsPremium =  false,
                IsEmailVerified =  false,
                OtpGeneratedAt =  DateTime.Now
            };

            await _usersRepository.AddAsync(user);
            await _usersRepository.SaveChangesAsync();

            await _mailService.SendEmailRegisterAccountAsync(
                        user.Email,
                        "Create Account Successfully!",
                        user.Username,
                        user.Password);
            var sendOtpEmailRequest = new SendOtpEmailRequest
            {
                Email = user.Email,
                UserName = user.Username,
            };
            await _mailService.SendEmailAsync(sendOtpEmailRequest);
            return ApiResult<UserResponse>.Ok(ToResponse(user));
            
        }

        public async Task<ApiResult<LoginResult>> LoginAsync(LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return ApiResult<LoginResult>.Fail("Email và Password là bắt buộc.");

            var user = await _usersRepository.GetByEmailAsync(req.Email.Trim());
            if (user == null)
                return ApiResult<LoginResult>.Fail("Sai email hoặc mật khẩu.");

            // plaintext theo yêu cầu
            if (!string.Equals(user.Password, req.Password))
                return ApiResult<LoginResult>.Fail("Sai email hoặc mật khẩu.");

            if (!(user.Status ?? false))
                return ApiResult<LoginResult>.Fail("Tài khoản đang bị khoá.");

            // ✅ Sinh JWT ngay tại service (controller không cần đụng repo)
            var token = await _jwtService.GenerateJwtTokenAsync(user, user.RoleId);

            var payload = new LoginResult
            {
                Token = token,
                Username = user.Username,
                RoleId = user.RoleId,
                userId = user.UserId,
                PictureProfile = user.PictureProfile,
                DateOfBirth = user.DateOfBirth,
                Phone = user.Phone,
                Status = user.Status,
                IsPremium = user.IsPremium,
                IsEmailVerified = user.IsEmailVerified,
                OtpGeneratedAt = user.OtpGeneratedAt
    };

            return ApiResult<LoginResult>.Ok(payload);
        }

        private static UserResponse ToResponse(User u) => new()
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            RoleId = u.RoleId,
            PictureProfile = u.PictureProfile,
            DateOfBirth = (u.DateOfBirth == default) ? null : u.DateOfBirth,
            Phone = u.Phone,
            Status = u.Status ?? true,
            IsPremium = u.IsPremium ?? false,
            IsEmailVerified = u.IsEmailVerified ?? false
        };

        public async Task AddAsync(Role role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (string.IsNullOrWhiteSpace(role.RoleName))
                throw new ArgumentException("Role.Name là bắt buộc.", nameof(role));

            role.RoleName = role.RoleName.Trim();

            if (await _roleRepository.ExistsByNameAsync(role.RoleName))
                throw new InvalidOperationException("Role name đã tồn tại.");

            await _roleRepository.AddAsync(role);          
            await _usersRepository.SaveChangesAsync();        
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            var user = await _usersRepository.GetUserByUserID(userId);
            return user;
        }
    }
}
