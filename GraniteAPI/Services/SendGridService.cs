using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

namespace GraniteAPI.Services
{
    public class SendGridService
    {
        private readonly string _apiKey;
        private readonly string _defaultToEmail;

        public SendGridService(IConfiguration config)
        {
            _apiKey = config["SendGrid:ApiKey"];
            _defaultToEmail = config["SendGrid:DefaultToEmail"]; // your admin email
        }

        public async Task SendDynamicEmailAsync(
            string fromEmail,
            string fromName,
            string subject,
            string htmlBody)
        {
            var client = new SendGridClient(_apiKey);

            var from = new EmailAddress(fromEmail, fromName);    // DYNAMIC FROM
            var to = new EmailAddress(_defaultToEmail);          // STATIC TO (admin)

            var message = MailHelper.CreateSingleEmail(
                from, to, subject, "", htmlBody
            );

            var response = await client.SendEmailAsync(message);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Body.ReadAsStringAsync();
                throw new Exception("SendGrid error: " + err);
            }
        }
    }
}
