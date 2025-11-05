using BusinessObject.DTO.Email;
using BusinessObject.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using Repo.IRepository;
using Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepo;

        public EmailService(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepo = userRepository;
        }

        public async Task<bool> CheckOtpEmail(CheckOtpRequest checkOtpRequest)
        {
            var user = await _userRepo.GetUserByEmailAsync(checkOtpRequest.Email);
            var otpEmail = await _userRepo.GetOTPEmailByEmail(checkOtpRequest.Email);
            if (otpEmail == null)
            {
                throw new Exception("OTP không đúng. Vui lòng nhập lại");
            }
            if (otpEmail.EndTime < DateTime.Now)
            {
                throw new Exception("OTP đã hết hạn");
            }
            if (!otpEmail.OtpKey.Equals(checkOtpRequest.OtpRequest))
            {
                return false;
            }
            user.IsEmailVerified = true;
            user.Status = true;
            await _userRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendEmailAsync(SendOtpEmailRequest sendEmailRequest)
        {
            var otpKey = GenerateOTP(6);
            var emailBody = _configuration["EmailSetting:EmailBody"];
            emailBody = emailBody.Replace("{PROJECT_NAME}", _configuration["Project_HomeTrack:PROJECT_NAME"]);
            emailBody = emailBody.Replace("{FULL_NAME}", sendEmailRequest.UserName);
            emailBody = emailBody.Replace("{EXPIRE_TIME}", "2");
            emailBody = emailBody.Replace("{OTP}", otpKey);
            emailBody = emailBody.Replace("{PHONE_NUMBER}", _configuration["Project_HomeTrack:PHONE_NUMBER"]);
            emailBody = emailBody.Replace("{EMAIL_ADDRESS}", _configuration["Project_HomeTrack:EMAIL_ADDRESS"]);
            var emailHost = _configuration["EmailSetting:EmailHost"];
            var userName = _configuration["EmailSetting:EmailUsername"];
            var password = _configuration["EmailSetting:EmailPassword"];
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(emailHost));
            email.To.Add(MailboxAddress.Parse(sendEmailRequest.Email));
            email.Subject = _configuration.GetSection("EmailSetting")?["Subject"];
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = emailBody
            };
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(emailHost, 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(userName, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            //InserDB
            OTPEmail otp = new OTPEmail()
            {
                Id = Guid.NewGuid(),
                Email = sendEmailRequest.Email,
                OtpKey = otpKey,
                CreatedTime = DateTime.Now,
                ExpireTime = 2,
            };
            otp.EndTime = otp.CreatedTime.GetValueOrDefault().AddMinutes(otp.ExpireTime ??= 2);
            var otpCheck = await _userRepo.AddOTPEmail(otp);
            if (otpCheck == null)
            {
                throw new Exception("Cannot insert otp to database");
            }
            return true;
        }

        private string GenerateOTP(int n)
        {
            string numbers = "1234567890";
            string otpKey = string.Empty;
            Random random = new Random();
            for (int i = 0; i < n; i++)
            {
                otpKey += numbers[random.Next(0, numbers.Length)];
            }
            return otpKey;
        }

        public async Task<bool> SendEmailRegisterAccountAsync(string emailRequest, string subjectEmail, string fullName, string passwordUser)
        {
            try
            {
                var emailBody = _configuration["EmailSetting:EmailRegisterAccount"];
                emailBody = emailBody.Replace("{PROJECT_NAME}", _configuration["EmailSetting:PROJECT_NAME"]);
                emailBody = emailBody.Replace("{FULL_NAME}", fullName);
                emailBody = emailBody.Replace("{EMAIL}", emailRequest);
                emailBody = emailBody.Replace("{PASSWORD}", passwordUser);
                emailBody = emailBody.Replace("{PHONE_NUMBER}", _configuration["EmailSetting:PHONE_NUMBER"]);
                emailBody = emailBody.Replace("{EMAIL_ADDRESS}", _configuration["EmailSetting:EMAIL_ADDRESS"]);

                var emailHost = _configuration["EmailSetting:EmailHost"];
                var userName = _configuration["EmailSetting:EmailUsername"];
                var password = _configuration["EmailSetting:EmailPassword"];
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(emailHost));
                email.To.Add(MailboxAddress.Parse(emailRequest));
                email.Subject = subjectEmail;
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = emailBody
                };
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(emailHost, 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(userName, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
