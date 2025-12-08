using System.Net;
using System.Net.Mail;

namespace GraniteAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string fromUserEmail, string subject, string body)
        {
            var toEmail = _config["EmailSettings:ToEmail"];

            var smtp = new SmtpClient
            {
                Host = _config["EmailSettings:Host"],
                Port = int.Parse(_config["EmailSettings:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["EmailSettings:Username"],
                    _config["EmailSettings:Password"]
                )
            };

            var message = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:FromEmail"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);
            message.ReplyToList.Add(new MailAddress(fromUserEmail));
            // Now admin can click “Reply” → reply directly to user

            await smtp.SendMailAsync(message);
        }
    }
}

