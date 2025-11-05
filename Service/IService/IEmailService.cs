using BusinessObject.DTO.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IEmailService
    {
        Task<bool> SendEmailRegisterAccountAsync(string emailRequest, string subjectEmail, string fullName, string passwordUser);
        Task<bool> CheckOtpEmail(CheckOtpRequest checkOtpRequest);
        Task<bool> SendEmailAsync(SendOtpEmailRequest sendEmailRequest);
    }
}
