using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MovieTicketMVC.Services
{
    public static class TicketEmailService
    {
        public static async Task SendTicketConfirmationAsync(
            string to,
            string subject,
            string body,
            byte[] attachmentBytes,
            string attachmentName)
        {
            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress("kinobiletara@gmail.com", "MovieTicket App");
                msg.To.Add(to);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = false;

                using (var ms = new MemoryStream(attachmentBytes))
                {
                    msg.Attachments.Add(new Attachment(ms, attachmentName));

                    using (var client = new SmtpClient())
                    {
                        await client.SendMailAsync(msg);
                    }
                }
            }
        }
    }
}