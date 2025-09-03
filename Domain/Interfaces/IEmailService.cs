using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Domain.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, List<IFormFile> attachments = null);
    }
}