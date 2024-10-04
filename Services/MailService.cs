using E_Commerce_Application___ASP.NET_MongoDB.Helpers;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public class MailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _domain;
        private readonly string _email;

        public MailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpClient = new SmtpClient
            {
                Host = smtpSettings.Value.Host,
                Port = smtpSettings.Value.Port,
                Credentials = new System.Net.NetworkCredential(smtpSettings.Value.Username, smtpSettings.Value.Password),
                EnableSsl = smtpSettings.Value.EnableSsl
            };
            _domain = smtpSettings.Value.Domain;
            _email = smtpSettings.Value.Username;
        }

        public async Task SendActivationEmail(string email, string activationToken)
        {
            var activationLink = $"{_domain}/activate?token={activationToken}&email={email}";
            var mailMessage = new MailMessage(_email, email)
            {
                Subject = "Activate Your Account",
                Body = $"Please click the following link to activate your account: <a href='{activationLink}'>Activate</a>",
                IsBodyHtml = true
            };

            await _smtpClient.SendMailAsync(mailMessage);
        }
    }
}
