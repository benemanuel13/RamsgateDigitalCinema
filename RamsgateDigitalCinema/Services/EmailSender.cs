using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MimeKit;
using MailKit.Net.Smtp;
//using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

using RamsgateDigitalCinema.Services;
using RamsgateDigitalCinema.Models;

using Microsoft.AspNetCore.Identity.UI.Services;

namespace RamsgateDigitalCinema.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly IWebHostEnvironment _env;

        public EmailSender(IOptions<EmailSettings> emailSettings, IWebHostEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient();

            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.Sender));
            mimeMessage.To.Add(MailboxAddress.Parse(email));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart("html") { Text = message };

            try
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.LocalDomain = "ramsgatedigitalcinema.co.uk";
                await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);
                await client.AuthenticateAsync(_emailSettings.Sender, _emailSettings.Password);
            }
            catch
            {
            }
            finally
            {
                client.Send(mimeMessage);
                client.Disconnect(true);
            }

            return;
        }
    }
}
