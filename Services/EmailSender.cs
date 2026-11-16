using System.Net;
using System.Net.Mail;

namespace FINALS.Services
{
    // Sends mail through Gmail's SMTP server
    public class EmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            string host = _config["Email:Host"] ?? "";
            int port = int.Parse(_config["Email:Port"] ?? "587");
            string from = _config["Email:From"] ?? "";
            string password = _config["Email:Password"] ?? "";
            string replyTo = _config["Email:ReplyTo"] ?? "";

            var message = new MailMessage();
            message.From = new MailAddress(from, "Venue Reservation System");
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            if (replyTo != "")
            {
                message.ReplyToList.Add(new MailAddress(replyTo));
            }

            var client = new SmtpClient(host, port);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(from, password);

            await client.SendMailAsync(message);
            client.Dispose();
        }
    }
}