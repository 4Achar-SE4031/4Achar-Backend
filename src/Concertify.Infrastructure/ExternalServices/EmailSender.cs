using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Concertify.Infrastructure.ExternalServices;

public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentNullException(nameof(email));
        }
        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentNullException(nameof(subject));
        }
        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentNullException(nameof(subject));
        }

        await Execute(email, subject, htmlMessage); 
    }

    private async Task Execute(string email, string subject, string htmlMessage)
    {
        string from = "concertifyteam@gmail.com";

        using var client = new SmtpClient();
        client.Host = "smtp.gmail.com";
        client.Port = 587;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.UseDefaultCredentials = false;
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential(from, "API_PASSWORD");
        using var message = new MailMessage(
            from: new MailAddress(from, "Concertify Team"),
            to: new MailAddress(email, email)
            );

        message.Subject = subject;
        message.Body = htmlMessage;

        await client.SendMailAsync(message);
    }
}
