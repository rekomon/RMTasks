
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using RMTasks.Models;

namespace RMTasks.Service
{
    public class EmailService : IEmailService
    {
 
        private readonly SMTPConfig _configuration;



        public EmailService(IOptions<SMTPConfig> options)
        {
            _configuration = options.Value;
        }



        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpClient = new SmtpClient
            {
                Host = _configuration.host,
                Port = _configuration.port,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_configuration.Username, _configuration.Password),
                EnableSsl = _configuration.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration.FromEmail),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }
       
    }
}
