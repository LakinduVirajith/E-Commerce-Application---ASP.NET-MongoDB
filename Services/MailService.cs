using E_Commerce_Application___ASP.NET_MongoDB.Helpers;
using E_Commerce_Application___ASP.NET_MongoDB.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public class MailService : IMailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _domain;
        private readonly string _compnayEmail;

        // CONSTRUCTOR TO SET UP SMTP CLIENT
        public MailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpClient = new SmtpClient
            {
                Host = smtpSettings.Value.Host,
                Port = smtpSettings.Value.Port,
                Credentials = new System.Net.NetworkCredential(smtpSettings.Value.Username, smtpSettings.Value.Password),
                EnableSsl = smtpSettings.Value.EnableSsl
            };

            // SET DOMAIN AND COMPANY EMAIL FROM SMTP SETTINGS
            _domain = smtpSettings.Value.Domain;
            _compnayEmail = smtpSettings.Value.Username;
        }

        // ASYNCHRONOUS METHOD TO SEND ACTIVATION EMAIL
        public async Task<bool> SendActivationEmailAsync(string toMail, string userName, string token)
        {
            try
            {
                var activationLink = $"{_domain}/api/v1/auth/activate?token={token}";  // ACTIVATION LINK

                string htmlBody = await File.ReadAllTextAsync("Templates/ActivationEmailTemplate.html");

                // REPLACE PLACEHOLDERS WITH USER NAME AND ACTIVATION LINK
                htmlBody = htmlBody.Replace("{userName}", userName);
                htmlBody = htmlBody.Replace("{activationLink}", activationLink);

                var mailMessage = new MailMessage(_compnayEmail, toMail)
                {
                    Subject = "Activate Your Account",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                await _smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
