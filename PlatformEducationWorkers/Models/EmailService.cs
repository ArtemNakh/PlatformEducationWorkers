//using System.Net.Mail;
//using System.Net;
//using Microsoft.AspNetCore.Mvc;
//using PlatformEducationWorkers.Core.Models;
//using PlatformEducationWorkers.Models;

//namespace PlatformEducationWorkers.Models
//{
//    public class EmailService
//    {
//        private readonly string _smtpServer;
//        private readonly int _port;
//        private readonly IConfiguration _configuration;
        
//        public EmailService(IConfiguration configuration)
//        {
//            _configuration = configuration;

//            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            
//            _port = Convert.ToInt32(_configuration["EmailSettings:Port"]);

//        }

//        public async Task SendEmailAsync(string senderEmail, string senderPassword,string toEmail, string subject, string body)
//        {
//            using (SmtpClient client = new SmtpClient(_smtpServer)) {
//                client.Credentials = new NetworkCredential(senderEmail, senderPassword);
//                client.Port = _port;
//                client.EnableSsl = true;

//                var mailMessage = new MailMessage
//                {
//                    From = new MailAddress(senderEmail),
//                    Subject = subject,
//                    Body = body,
//                    IsBodyHtml = true
//                };

//                mailMessage.To.Add(toEmail);

//                await client.SendMailAsync(mailMessage);
//            }
           
//        }
//    }
//}


