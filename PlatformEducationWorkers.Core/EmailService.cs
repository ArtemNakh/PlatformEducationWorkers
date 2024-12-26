using System.Net.Mail;
using System.Net;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core;
using Microsoft.Extensions.Configuration;

namespace PlatformEducationWorkers.Core
{
    /// <summary>
    /// Service for sending emails.
    /// </summary>
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor for EmailService.
        /// </summary>
        /// <param name="configuration">Configuration containing email settings.</param>
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;

            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            
            _port = Convert.ToInt32(_configuration["EmailSettings:Port"]);

        }

        /// <summary>
        /// Asynchronously sends an email.
        /// </summary>
        /// <param name="senderEmail">Sender's email address.</param>
        /// <param name="senderPassword">Password for the sender's email address.</param>
        /// <param name="toEmail">Recipient's email address.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="body">Body of the email.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task SendEmailAsync(string senderEmail, string senderPassword,string toEmail, string subject, string body)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(_smtpServer))
                {
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    client.Port = _port;
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }

            }
            catch (SmtpException smtpEx)
            {
                if (smtpEx.Message.Contains("5.7.0 Authentication Required"))
                {
                    throw new Exception("Помилка автентифікації: сервер вимагає захищене з'єднання або невірні облікові дані.");
                }

                throw new Exception("Помилка при надсиланні електронного листа: " + smtpEx.Message, smtpEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Загальна помилка при надсиланні електронного листа: " + ex.Message, ex);
            }


        }
    }
}


