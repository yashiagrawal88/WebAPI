using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        string confirmEmail = ConfigurationManager.AppSettings["confirmEmail"];
        private string GetTemplate(string fileName)
        {
            var filePath = _configuration["Email:HtmlRootPath"] + fileName;
            return File.ReadAllText(filePath);
        }

        public string ConfirmEmailMailBody
        {
            get
            {
                return GetTemplate("ConfirmEmailAddress.html");
            }
        }
        public async Task SendConfirmMail(string email, string Link, string loginUserName)
        {
            var msg = ConfirmEmailMailBody.Replace("{name}", loginUserName)
                .Replace("{link}", Link);
            await SendEmail(email, "Confirm Your Account", msg);
            //await userMan.SendEmailAsync(applicationUser.Id, "Confirm Your Account", msg);
        }
        public async Task SendEmail(string email, string subject, string body)
        {



            using (var client = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = _configuration["Email:Email"],
                    Password = _configuration["Email:Password"]
                };

                client.Credentials = credential;
                client.Host = _configuration["Email:Host"];
                client.Port = int.Parse(_configuration["Email:Port"]);
                client.EnableSsl = true;

                using (var emailMessage = new MailMessage())
                {
                    emailMessage.To.Add(new MailAddress(email));
                    emailMessage.From = new MailAddress(_configuration["Email:Email"]);
                    emailMessage.Subject = subject;
                    emailMessage.Body = body;
                    emailMessage.IsBodyHtml = true;
                    client.Send(emailMessage);
                }
            }
            await Task.CompletedTask;
        }


    }
}