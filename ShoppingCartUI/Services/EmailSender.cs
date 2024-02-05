using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ShoppingCartUI.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;

        public EmailSender(ILogger<EmailSender> logger, IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            _logger = logger;
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrEmpty(Options.TempPwd))
            {
                _logger.LogDebug("TempPwd is not set.");
                throw new Exception("TempPwd is not set.");
            }
            await Execute(Options.TempPwd, subject, message, toEmail);
        }

        private async Task Execute(string tempPwd, string subject, string message, string toEmail)
        {
            // 使用 Google Mail Server 發信
            string GoogleID = Options.SenderEmail ?? throw new Exception("SenderEmail is not set."); //Google 發信帳號
            //string TempPwd = "xxxxxxxxxxxxxxxx"; //應用程式密碼
            //string ReceiveMail = "xxxxx@yahoo.com.tw"; //接收信箱

            string SmtpServer = "smtp.gmail.com";
            int SmtpPort = 587;
            MailMessage mms = new MailMessage();
            mms.From = new MailAddress(GoogleID);
            mms.Subject = subject;
            mms.Body = message;
            mms.IsBodyHtml = true;
            mms.SubjectEncoding = Encoding.UTF8;
            mms.To.Add(new MailAddress(toEmail));
            using (SmtpClient client = new SmtpClient(SmtpServer, SmtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(GoogleID, Options.TempPwd);//寄信帳密
                await client.SendMailAsync(mms); //寄出信件
            }
        }
    }
}